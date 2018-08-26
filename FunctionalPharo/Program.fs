// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open System.Windows.Forms
open System.Drawing
open SkiaSharp.Views.Desktop
open SkiaSharp

type SkiaForm() as form = 
    inherit Form()

    let skiaView = new SkiaSharp.Views.Desktop.SKControl()

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
        let scaledSize = new SKSize((float32 args.Info.Width), (float32 args.Info.Height))
        canvas.Clear(SKColors.White)
        let paint = new SKPaint()
        paint.Color <- SKColors.Black
        paint.IsAntialias <- true
        paint.Style <- SKPaintStyle.Fill
        paint.TextAlign <- SKTextAlign.Center
        paint.TextSize <- (float32 24.0)
        let coord = new SKPoint (scaledSize.Width / (float32 2.0), (scaledSize.Height + paint.TextSize) / (float32 2.0))
        canvas.DrawText("SkiaSharp", coord, paint)
        ()



[<EntryPoint>]
[<STAThread>]
let main argv = 
    Application.SetCompatibleTextRenderingDefault false
    use form = new SkiaForm()
    Application.Run form
    0 // return an integer exit code
