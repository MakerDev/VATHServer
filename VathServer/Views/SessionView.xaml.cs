using Microsoft.Maui.Controls;
using System;
using Microsoft.Maui.Graphics;
using System;
using System.Linq;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using VathServer.ViewModels;

namespace VathServer;

// https://www.mongodb.com/community/forums/t/maui-app-crashes-when-built-in-release-mode/181801/3
// 이거 없으면 릴리스에서만 크래시남.
[XamlCompilation(XamlCompilationOptions.Skip)] 
public partial class SessionView : ContentPage
{

    public SessionView(SessionViewModel sessionViewModel)
    {
        InitializeComponent();
        BindingContext = sessionViewModel;
    }
}

