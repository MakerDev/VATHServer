using Microsoft.Maui.Controls;
using System;
using Microsoft.Maui.Graphics;
using System;
using System.Linq;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using VathServer.ViewModels;

namespace VathServer;

public partial class SessionView : ContentPage
{

    public SessionView(SessionViewModel sessionViewModel)
    {
        InitializeComponent();
        BindingContext = sessionViewModel;
    }
}

