module State

type WidgetData = {
    left: float32; 
    top: float32;
    width: float32;
    height: float32;
}

type TextData = {
    text: string;
}

type InputFieldData = {
    value: string;
    onChange: string -> unit
}

type Widget = 
    | Panel of WidgetData * Widget array
    | Text of WidgetData * TextData
    | InputField of WidgetData * InputFieldData

type AppState = WidgetTree 