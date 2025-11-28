using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using VanGest.Server.Services;

namespace VanGest.Server.Services
{
    public class ModalService
    {
        public event Action<string, RenderFragment, ModalOptions>? OnShow;
        public event Action? OnClose;

        public void Show(string title, RenderFragment content, ModalOptions options)
        {
            OnShow?.Invoke(title, content, options);
        }

        public void Close()
        {
            OnClose?.Invoke();
        }

        public class ModalOptions
        {
            public bool Fullscreen { get; set; }
        }
    }
}