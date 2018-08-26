module Render

open SkiaSharp

type Point = float32 * float32
type Size = float32 * float32

type ColorAndWidth = SKColor * float32

type BorderSettings = 
    | Left of ColorAndWidth
    | Top of ColorAndWidth
    | Right of ColorAndWidth
    | Bottom of ColorAndWidth

type PaintSettings = {
    fillColor: Option<SKColor>;
    borderLeft: Option<ColorAndWidth>;
    borderTop: Option<ColorAndWidth>;
    borderRight: Option<ColorAndWidth>;
    borderBottom: Option<ColorAndWidth>
}

type Geometry = 
    | Rectangle of size:Size
    | Circle of radius: float32
    | Triangle of Point * Point * Point

type TextSettings = {
    fontSize: int;
    fontFamily: string;
    color: string;
}    

type Node = 
    | Panel of Point * Size * Node array
    | Shape of Point * PaintSettings * Geometry
    | Text of Point * TextSettings * string

let toSkPoint((x,y): Point) = 
    new SKPoint(x,y)

let fillShape (canvas: SKCanvas, paintSettings: PaintSettings, geometry: Geometry): unit = 
    match paintSettings.fillColor with
    | Some(color) -> 
        let skPaint = new SKPaint()
        skPaint.Color <- color
        skPaint.IsAntialias <- true
        match geometry with 
        | Rectangle (width, height) -> 
            canvas.DrawRect(0.f,0.f,width,height, skPaint)
        | Circle radius -> 
            canvas.DrawCircle(0.f,0.f,radius, skPaint)
        | Triangle(p1, p2, p3) ->  
            canvas.DrawPoints(SKPointMode.Polygon, Array.map toSkPoint [| p1; p2; p3 |], skPaint)
    | None -> ()

let drawBorders (canvas: SKCanvas, paintSettings: PaintSettings, geometry: Geometry) = 
    ()
               
let rec paint (canvas: SKCanvas, widget: Node) = 
    canvas.Save() |> ignore
    match widget with
    | Panel((x,y), (width,height), children) ->
        canvas.Translate(x,y)
        canvas.ClipRect(SKRect.Create(width,height))
        for child in children do
            paint(canvas, child)
    | Shape((x,y), paintSettings, geometry) ->
        canvas.Translate(x,y)
        fillShape(canvas, paintSettings, geometry)
        drawBorders(canvas, paintSettings, geometry)
    canvas.Restore() |> ignore

let fillBlackNoBorder: PaintSettings = 
    {
        fillColor = Some(SKColors.Black);
        borderLeft = None;
        borderRight = None;
        borderTop = None;
        borderBottom = None;
    }