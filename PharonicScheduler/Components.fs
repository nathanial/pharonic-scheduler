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
    fontSize: int;
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

type Widget = 
    | Panel of WidgetData * Widget array
    | Text of WidgetData * TextData
    | InputField of WidgetData * InputFieldData

type ComputedSize = 
    | Size of float32 * float32
    | CouldNotComputeSize

    member this.Width = 
        match this with 
        | Size(width,height) -> width
        | CouldNotComputeSize -> raise(new Exception("Cannot get width of 'CouldNotComputeSize'"))

    member this.Height =
        match this with
        | Size(width,height) -> height
        | CouldNotComputeSize -> raise(new Exception("Cannot get height of 'CouldNotComputeSize'"))

type SizedWidget = 
    | SizedPanel of ComputedSize * WidgetData * SizedWidget array
    | SizedText of ComputedSize * WidgetData * TextData
    | SizedInputField of ComputedSize * WidgetData * InputFieldData

    member this.Size = 
        match this with 
        | SizedPanel(size, data, children) -> size
        | SizedText(size, data, textData) -> size
        | SizedInputField(size, data, inputFieldData) -> size

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

let rec toSizeTree(widget: Widget, parentSize: ComputedSize): SizedWidget =
    match widget with 
    | Panel(data, children) -> 
        match data.size with 
        | FixedSize(width, height) -> 
            let computedSize = resolveSize(width, height, parentSize)
            let sizedChildren = children |> Array.map(fun child -> toSizeTree(child, computedSize))
            SizedPanel(computedSize, data, sizedChildren)
        | SizeToContent -> 
            let sizedChildren = children |> Array.map(fun child -> toSizeTree(child, parentSize))           
            let maxWidth = sizedChildren |> Array.map(fun child -> child.Size.Width) |> Array.max
            let maxHeight = sizedChildren |> Array.map(fun child -> child.Size.Height) |> Array.max
            SizedPanel(Size(maxWidth, maxHeight), data, sizedChildren)
    | Text(data, textData) ->
        match data.size with 
        | FixedSize(width, height) ->
            let computedSize = resolveSize(width, height, parentSize)
            SizedText(computedSize, data, textData)
        | SizeToContent -> 
            let paint = new SKPaint()
            paint.IsAntialias <- true
            paint.TextEncoding <- SKTextEncoding.Utf8
            paint.TextSize <- 45.f
            paint.Typeface <- SKFontManager.Default.MatchCharacter("Times New Roman", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright, null, ' ');
            paint.Color <- SKColors.Black
            let metrics = ref(new SKFontMetrics())
            let bounds = ref(new SKRect())
            paint.MeasureText(textData.text, bounds) |> ignore
            SizedText(Size(bounds.Value.Width, bounds.Value.Height), data, textData)
    | InputField(data, inputData) -> 
        match data.size with 
        | FixedSize(width, height) -> raise(new Exception("Fixed Size Not Supported Yet"))
        | SizeToContent -> raise(new Exception("SizeToContent Not Supported Yet"))

let rec sizedToRenderNode sizedWidget = 
    match sizedWidget with 
    | SizedPanel(size, data, children) -> 
        match size with 
        | Size(width,height) -> 
            Render.Panel((0.f,0.f), (width,height), children |> Array.map(sizedToRenderNode))
        | CouldNotComputeSize -> raise(new Exception("Could not compute size"))
    | SizedText(size, data, textData) ->
        match size with 
        | Size(width,height) -> 
            let settings: TextSettings = {
                fontSize=textData.fontSettings.fontSize;
                fontFamily=textData.fontSettings.fontFamily;
                color=textData.fontSettings.color;
            }
            Render.Text((0.f,0.f), (width, height), settings, textData.text)
        | CouldNotComputeSize -> raise(new Exception("Could not compute size"))
    | SizedInputField(size, data, inputData) ->
        match size with 
        | Size(x,y) -> raise(new Exception(String.Format("Got A Size {0},{1}", x, y)))
        | CouldNotComputeSize -> raise(new Exception("Could not compute size"))


let toRenderTree(widget: Widget, parentWidth: float32, parentHeight: float32): Render.Node = 
    toSizeTree(widget, Size(parentWidth, parentHeight)) |> sizedToRenderNode

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
        fontSettings={ fontSize=16; fontFamily="Consolas"; color=SKColor.Parse("#000000");}
    })