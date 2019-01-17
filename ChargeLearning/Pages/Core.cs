using Blazor.Extensions;
using Microsoft.AspNetCore.Blazor.Components;
using System;

namespace ChargeLearning.Pages
{
    public class Core : BlazorComponent
    {
        protected BECanvasComponent _canvas;
        private Canvas2dContext _ctx;

        //[Inject]
        //protected HttpClient Http { get; set; }

        protected override void OnAfterRender()
        {
            _ctx = _canvas.CreateCanvas2d();
            Console.WriteLine("Canvas happening");
            _ctx.FillStyle = "gray";
            _ctx.FillRect(0, 0, 500, 500);
        }

        public void Frame()
        {
            _ctx.FillStyle = "black";
            _ctx.FillRect(0, 0, 500, 500);
        }
        
    }
}
