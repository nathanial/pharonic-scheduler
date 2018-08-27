module Components

open System
open SkiaSharp
open Render

type Coordinate =
    | Percentage of float32
    | Pixels of float32
    | Zero

type LocationValue = Coordinate
type SizeValue = Coordinate

type BoundsData = {
    x: LocationValue;
    y: LocationValue;
    width: SizeValue;
    height: SizeValue;
}

type VAlignment = 
    | Top 
    | Bottom
    | VCenter

type HAlignment = 
    | Left 
    | Right
    | HCenter

type Anchor = VAlignment * HAlignment

type Offset = LocationValue * LocationValue

type WidgetSize =
    | FixedSize of SizeValue * SizeValue
    | SizeToContent

type WidgetData = {
    offset: Offset
    anchor: Anchor;
    size: WidgetSize;
}

type FontSettings = {
    fontSize: float32;
    fontFamily: string;
    color: SKColor;
}

type TextData = {
    text: string;
    fontSettings: FontSettings;
}

type InputFieldData = {
    value: string;
    onChange: string -> unit
}

type Tree<'a> = {
    data: 'a;
    children: Tree<'a> array;
}

type Widget = 
    | Panel of WidgetData * Widget array
    | Text of WidgetData * TextData
    | InputField of WidgetData * InputFieldData

type ComputedSize = 
    | Size of float32 * float32
    | CouldNotComputeSize

type SizedWidget = {
    size:ComputedSize;
    lineHeight:Option<float32>;
    widget:Widget;
}

let resolveSize(width: SizeValue, height: SizeValue, parentSize: ComputedSize): ComputedSize = 
    let finalWidth = 
        match width with
        | Zero -> 0.f
        | Pixels(w) -> w
        | Percentage(w) ->
            match parentSize with 
            | Size(parentWidth, parentHeight) -> w / 100.0f * parentWidth
            | CouldNotComputeSize -> raise(new Exception("Percentage without parent size"))
    let finalHeight = 
        match height with 
        | Zero -> 0.f
        | Pixels(h) -> h
        | Percentage(h) ->
            match parentSize with 
            | Size(parentWidth, parentHeight) -> h / 100.0f * parentHeight
            | CouldNotComputeSize -> raise(new Exception("Percentage without parent size"))
    Size(finalWidth, finalHeight)


let maxWidth(children: Tree<SizedWidget> seq) = Array.max [| for {data={size=Size(w,_)}} in children do yield w |]
let maxHeight(children: Tree<SizedWidget> seq) = Array.max [| for {data={size=Size(_,h)}} in children do yield h |]

let rec toSizeTree(widget: Widget, parentSize: ComputedSize): Tree<SizedWidget> =
    match widget with 
    | Panel(data, children) -> 
        match data.size with 
        | FixedSize(width, height) -> 
            let computedSize = resolveSize(width, height, parentSize)
            let sizedChildren = children |> Array.map(fun child -> toSizeTree(child, computedSize))
            { data = {size=computedSize; lineHeight=None; widget=widget}; children = sizedChildren }
        | SizeToContent -> 
            let sizedChildren = children |> Array.map(fun child -> toSizeTree(child, parentSize))           
            let computedSize = Size(maxWidth(sizedChildren), maxHeight(sizedChildren))
            { data = {size=computedSize; lineHeight=None; widget=widget}; children = sizedChildren }
    | Text(data, textData) ->
        match data.size with 
        | FixedSize(width, height) ->
            let computedSize = resolveSize(width, height, parentSize)
            { data = {size=computedSize; lineHeight=None; widget=widget}; children = [||] }
        | SizeToContent -> 
            let paint = new SKPaint()
            paint.IsAntialias <- true
            paint.TextEncoding <- SKTextEncoding.Utf8
            paint.TextSize <- textData.fontSettings.fontSize
            paint.Typeface <- SKFontManager.Default.MatchCharacter(
                textData.fontSettings.fontFamily, 
                SKFontStyleWeight.Normal, 
                SKFontStyleWidth.Normal, 
                SKFontStyleSlant.Upright, 
                null, 
                ' '
            )
            paint.Color <- SKColors.Black
            let metrics = ref(new SKFontMetrics())
            let bounds = ref(new SKRect())
            paint.MeasureText(textData.text, bounds) |> ignore
            paint.GetFontMetrics(metrics) |> ignore
            let maxHeight =  metrics.Value.Bottom -  metrics.Value.Top
            let height = bounds.Value.Height
            let computedSize = Size(bounds.Value.Width, maxHeight)
            { data = {size=computedSize; lineHeight=Some(height); widget=widget}; children = [||] }
    | InputField(data, inputData) -> 
        match data.size with 
        | FixedSize(width, height) -> raise(new Exception("Fixed Size Not Supported Yet"))
        | SizeToContent -> raise(new Exception("SizeToContent Not Supported Yet"))

let alignmentToPosition((valign, halign),(elementWidth, elementHeight),(parentWidth, parentHeight)) = 
    let x = 
        match halign with 
        | Left -> 0.f
        | Right -> float32(parentWidth - elementWidth)
        | HCenter -> (parentWidth / 2.f) - (elementWidth / 2.f)
    let y = 
        match valign with 
        | Top -> 0.f
        | Bottom -> float32(parentHeight - elementHeight)
        | VCenter -> (parentHeight / 2.f) - (elementHeight / 2.f)
    (x,y)

let rec sizedToRenderNode(tree: Tree<SizedWidget>, parentSize: float32 * float32) = 
    let {size=size;lineHeight=lineHeight;widget=widget} = tree.data
    match size with 
    | Size(width, height) ->
        match widget with 
        | Panel(data, children) ->
            let pos = alignmentToPosition(data.anchor, (width, height), parentSize)
            Render.Panel(pos, (width,height), tree.children |> Array.map(fun c -> sizedToRenderNode(c, (width,height))))
        | Text(data, textData) ->
            let settings: TextSettings = {
                fontSize=textData.fontSettings.fontSize;
                fontFamily=textData.fontSettings.fontFamily;
                color=textData.fontSettings.color;
            }
            let pos = alignmentToPosition(data.anchor, (width, height), parentSize)
            Render.Text(pos, (width, height), lineHeight.Value, settings, textData.text)
        | InputField(data, inputData) -> 
            raise(new Exception("Input Field Not Implemented"))
    | CouldNotComputeSize ->
        raise(new Exception("Could not compute size"))

let toRenderTree(widget: Widget, parentWidth: float32, parentHeight: float32): Render.Node = 
    let sizeTree = toSizeTree(widget, Size(parentWidth, parentHeight))
    sizedToRenderNode(sizeTree, (parentWidth, parentHeight))

let anchorCenter = (VCenter, HCenter)
let noOffset = (Zero, Zero)
let defaultWidgetData = {
    offset=noOffset;
    anchor=anchorCenter;
    size=SizeToContent
}

let panel(children: Widget array): Widget = 
    Panel(defaultWidgetData, children)

let text(value: string): Widget = 
    Text(defaultWidgetData, {
        text=value;
        fontSettings={ fontSize=48.0f; fontFamily="Consolas"; color=SKColor.Parse("#000000");}
    })