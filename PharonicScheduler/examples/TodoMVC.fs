module TodoMVC

open Components

type Todo = {
    complete: bool;
    title: string;
}

type Model = {
    todos: Todo array
}

let render(model: Model): Components.Widget = 
    panel([| text("Magic Saucey") |])