module Program

open System
open System.Windows.Forms
open System.Drawing
open SkiaSharp.Views.Desktop
open SkiaSharp
open System.Collections

type SkiaForm() as form = 
    inherit Form()

    let skiaView = new SkiaSharp.Views.Desktop.SKControl()
    let model: TodoMVC.Model = { todos=[||] }
        
    let renderDebugText(canvas: SKCanvas) = 
        let skPaint = new SKPaint()
        skPaint.IsAntialias <- true
        skPaint.HintingLevel <- SKPaintHinting.Full
        skPaint.Color <- SKColors.Black
        skPaint.TextSize <- 72.0f;
        skPaint.IsLinearText <- true
        skPaint.TextScaleX <- 1.0f;
        skPaint.TextSkewX <- 0.0f;
        skPaint.StrokeWidth <- 0.0f;
        skPaint.Typeface <- SKFontManager.Default.MatchCharacter("Times New Roman", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright, null, ' ');
        let metrics = new SKFontMetrics()
        canvas.DrawText("SkiaSharp", new SKPoint(100.f,100.f), skPaint)

    do form.InitializeForm()

    member this.InitializeForm () =
        skiaView.Dock <- DockStyle.Fill
        skiaView.Location <- new Point(0,0)
        skiaView.Size <- new Size(774,529)
        form.ClientSize <- new Size(774, 529)
        form.Controls.Add(skiaView)

        form.AutoScaleMode <- AutoScaleMode.None

        let PaintHandler (evArgs: SKPaintSurfaceEventArgs) = 
            this.PaintSurface(evArgs)
        skiaView.PaintSurface.Add(PaintHandler)
 
    member this.PaintSurface (args: SKPaintSurfaceEventArgs): unit = 
        let canvas = args.Surface.Canvas
        canvas.Clear(SKColors.White)
        // renderDebugText(canvas)
        let root: Render.Node = Components.toRenderTree(TodoMVC.render(model), float32(args.Info.Width), float32(args.Info.Height))
        Render.paint(canvas, root)


[<EntryPoint>]
[<STAThread>]
let main argv = 
    Application.SetCompatibleTextRenderingDefault false
    use form = new SkiaForm()
    Application.Run form
    0 // return an integer exit code
