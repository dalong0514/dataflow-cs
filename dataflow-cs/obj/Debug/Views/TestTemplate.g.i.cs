﻿#pragma checksum "..\..\..\Views\TestTemplate.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "7D7D321C28F5F3B88AE87BE84BC25F3B6338EFE1B457D645F5482E8F69DE3726"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
using dataflow_cs.ViewModel;
using dataflow_cs.Views;


namespace dataflow_cs.Views {
    
    
    /// <summary>
    /// TestTemplate
    /// </summary>
    public partial class TestTemplate : MahApps.Metro.Controls.MetroWindow, System.Windows.Markup.IComponentConnector {
        
        
        #line 85 "..\..\..\Views\TestTemplate.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox DataTypeComboBox;
        
        #line default
        #line hidden
        
        
        #line 103 "..\..\..\Views\TestTemplate.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock DataCountTextBlock;
        
        #line default
        #line hidden
        
        
        #line 118 "..\..\..\Views\TestTemplate.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock DataStatusTextBlock;
        
        #line default
        #line hidden
        
        
        #line 133 "..\..\..\Views\TestTemplate.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SelectButton;
        
        #line default
        #line hidden
        
        
        #line 148 "..\..\..\Views\TestTemplate.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SelectAllButton;
        
        #line default
        #line hidden
        
        
        #line 163 "..\..\..\Views\TestTemplate.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ExportButton;
        
        #line default
        #line hidden
        
        
        #line 178 "..\..\..\Views\TestTemplate.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button CancelButton;
        
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
            System.Uri resourceLocater = new System.Uri("/dataflow-cs;component/views/testtemplate.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Views\TestTemplate.xaml"
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
            this.DataTypeComboBox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 2:
            this.DataCountTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.DataStatusTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.SelectButton = ((System.Windows.Controls.Button)(target));
            
            #line 137 "..\..\..\Views\TestTemplate.xaml"
            this.SelectButton.Click += new System.Windows.RoutedEventHandler(this.SelectButton_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.SelectAllButton = ((System.Windows.Controls.Button)(target));
            
            #line 152 "..\..\..\Views\TestTemplate.xaml"
            this.SelectAllButton.Click += new System.Windows.RoutedEventHandler(this.SelectAllButton_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.ExportButton = ((System.Windows.Controls.Button)(target));
            
            #line 167 "..\..\..\Views\TestTemplate.xaml"
            this.ExportButton.Click += new System.Windows.RoutedEventHandler(this.ExportButton_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.CancelButton = ((System.Windows.Controls.Button)(target));
            
            #line 182 "..\..\..\Views\TestTemplate.xaml"
            this.CancelButton.Click += new System.Windows.RoutedEventHandler(this.CancelButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

