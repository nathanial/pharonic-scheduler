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
    let root: Render.Node = Render.Panel((0.0f,0.0f), (100.0f,100.0f),
        [|
            Render.Shape((10.0f, 10.0f), Render.fillBlackNoBorder, Render.Rectangle((20.f, 20.f)));
            Render.Shape((30.0f, 30.0f), Render.fillBlackNoBorder, Render.Rectangle((30.f, 30.f)));
        |])

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
        Render.paint(canvas, root)


[<EntryPoint>]
[<STAThread>]
let main argv = 
    Application.SetCompatibleTextRenderingDefault false
    use form = new SkiaForm()
    Application.Run form
    0 // return an integer exit code
