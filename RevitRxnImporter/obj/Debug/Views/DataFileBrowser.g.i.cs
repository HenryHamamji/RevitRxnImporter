﻿#pragma checksum "..\..\..\Views\DataFileBrowser.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "5FDD0B12EC95DEA9714C2F8A3822A89C"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
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


namespace RevitReactionImporter {
    
    
    /// <summary>
    /// DataFileBrowser
    /// </summary>
    public partial class DataFileBrowser : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 9 "..\..\..\Views\DataFileBrowser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtEditorRAMModel;
        
        #line default
        #line hidden
        
        
        #line 10 "..\..\..\Views\DataFileBrowser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnRAMModelFile;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\..\Views\DataFileBrowser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtEditorRAMReactions;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\Views\DataFileBrowser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnRAMReactionsFile;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\Views\DataFileBrowser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtEditorRAMStuds;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\Views\DataFileBrowser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnRAMStudsFile;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\Views\DataFileBrowser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtEditorRAMCamber;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\Views\DataFileBrowser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnRAMCamberFile;
        
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
            System.Uri resourceLocater = new System.Uri("/RevitReactionImporterApp;component/views/datafilebrowser.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Views\DataFileBrowser.xaml"
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
            this.txtEditorRAMModel = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            this.btnRAMModelFile = ((System.Windows.Controls.Button)(target));
            
            #line 10 "..\..\..\Views\DataFileBrowser.xaml"
            this.btnRAMModelFile.Click += new System.Windows.RoutedEventHandler(this.onBrowseFileClick);
            
            #line default
            #line hidden
            return;
            case 3:
            this.txtEditorRAMReactions = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.btnRAMReactionsFile = ((System.Windows.Controls.Button)(target));
            
            #line 13 "..\..\..\Views\DataFileBrowser.xaml"
            this.btnRAMReactionsFile.Click += new System.Windows.RoutedEventHandler(this.onBrowseFileClick);
            
            #line default
            #line hidden
            return;
            case 5:
            this.txtEditorRAMStuds = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.btnRAMStudsFile = ((System.Windows.Controls.Button)(target));
            
            #line 16 "..\..\..\Views\DataFileBrowser.xaml"
            this.btnRAMStudsFile.Click += new System.Windows.RoutedEventHandler(this.onBrowseFileClick);
            
            #line default
            #line hidden
            return;
            case 7:
            this.txtEditorRAMCamber = ((System.Windows.Controls.TextBox)(target));
            return;
            case 8:
            this.btnRAMCamberFile = ((System.Windows.Controls.Button)(target));
            
            #line 19 "..\..\..\Views\DataFileBrowser.xaml"
            this.btnRAMCamberFile.Click += new System.Windows.RoutedEventHandler(this.onBrowseFileClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
