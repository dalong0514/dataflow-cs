﻿#pragma checksum "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "259CB7896F1E031B2AC108E8DF4C798B0890607C6E1A79908649B2554B4BDDBE"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
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
using dataflow_cs.Business.GsLc.Views;


namespace dataflow_cs.Business.GsLc.Views {
    
    
    /// <summary>
    /// UserControl1
    /// </summary>
    public partial class UserControl1 : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 29 "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ComboBoxDataType;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TextBlockDataCount;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TextBlockDataStatus;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonSelect;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonSelectAll;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonExport;
        
        #line default
        #line hidden
        
        
        #line 56 "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonCancel;
        
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
            System.Uri resourceLocater = new System.Uri("/dataflow-cs;component/business/gslc/views/usercontrol1.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml"
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
            
            #line 20 "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CloseButton_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.ComboBoxDataType = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 3:
            this.TextBlockDataCount = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.TextBlockDataStatus = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.ButtonSelect = ((System.Windows.Controls.Button)(target));
            
            #line 53 "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml"
            this.ButtonSelect.Click += new System.Windows.RoutedEventHandler(this.ButtonSelect_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.ButtonSelectAll = ((System.Windows.Controls.Button)(target));
            
            #line 54 "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml"
            this.ButtonSelectAll.Click += new System.Windows.RoutedEventHandler(this.ButtonSelectAll_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.ButtonExport = ((System.Windows.Controls.Button)(target));
            
            #line 55 "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml"
            this.ButtonExport.Click += new System.Windows.RoutedEventHandler(this.ButtonExport_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.ButtonCancel = ((System.Windows.Controls.Button)(target));
            
            #line 56 "..\..\..\..\..\Business\GsLc\Views\UserControl1.xaml"
            this.ButtonCancel.Click += new System.Windows.RoutedEventHandler(this.ButtonCancel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

