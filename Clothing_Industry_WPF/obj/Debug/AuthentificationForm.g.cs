﻿#pragma checksum "..\..\AuthentificationForm.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "F5481CD050842BEA996780D0DA6C21ADB3743B668364E38334A6AD047557D52D"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Clothing_Industry_WPF {
    
    
    /// <summary>
    /// AuthentificationForm
    /// </summary>
    public partial class AuthentificationForm : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 142 "..\..\AuthentificationForm.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Button_LogIn;
        
        #line default
        #line hidden
        
        
        #line 143 "..\..\AuthentificationForm.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonExit;
        
        #line default
        #line hidden
        
        
        #line 144 "..\..\AuthentificationForm.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox textboxLogin;
        
        #line default
        #line hidden
        
        
        #line 147 "..\..\AuthentificationForm.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox PasswordBoxPassword;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Clothing_Industry_WPF;component/authentificationform.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\AuthentificationForm.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.Button_LogIn = ((System.Windows.Controls.Button)(target));
            
            #line 142 "..\..\AuthentificationForm.xaml"
            this.Button_LogIn.Click += new System.Windows.RoutedEventHandler(this.Button_LogIn_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.ButtonExit = ((System.Windows.Controls.Button)(target));
            
            #line 143 "..\..\AuthentificationForm.xaml"
            this.ButtonExit.Click += new System.Windows.RoutedEventHandler(this.ButtonExit_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.textboxLogin = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.PasswordBoxPassword = ((System.Windows.Controls.PasswordBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

