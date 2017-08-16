﻿#pragma checksum "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "C0F1579FD1C233A9F077EE016CEB53F3"
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
    /// AnnotationTypeSelectionForVisualization
    /// </summary>
    public partial class AnnotationTypeSelectionForVisualization : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 18 "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button VisualizeRAMReactions;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock VisualizeReactionsText;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button VisualizeRAMSizes;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock VisualizeSizesText;
        
        #line default
        #line hidden
        
        
        #line 63 "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button VisualizeRAMStuds;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock VisualizeStudCountsText;
        
        #line default
        #line hidden
        
        
        #line 86 "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button VisualizeRAMCamber;
        
        #line default
        #line hidden
        
        
        #line 98 "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock VisualizeCamberText;
        
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
            System.Uri resourceLocater = new System.Uri("/RevitReactionImporterApp;component/views/annotationtypeselectionforvisualization" +
                    ".xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml"
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
            this.VisualizeRAMReactions = ((System.Windows.Controls.Button)(target));
            
            #line 24 "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml"
            this.VisualizeRAMReactions.Click += new System.Windows.RoutedEventHandler(this.OnAnnotationToVisualizeClick);
            
            #line default
            #line hidden
            return;
            case 2:
            this.VisualizeReactionsText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.VisualizeRAMSizes = ((System.Windows.Controls.Button)(target));
            
            #line 46 "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml"
            this.VisualizeRAMSizes.Click += new System.Windows.RoutedEventHandler(this.OnAnnotationToVisualizeClick);
            
            #line default
            #line hidden
            return;
            case 4:
            this.VisualizeSizesText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.VisualizeRAMStuds = ((System.Windows.Controls.Button)(target));
            
            #line 69 "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml"
            this.VisualizeRAMStuds.Click += new System.Windows.RoutedEventHandler(this.OnAnnotationToVisualizeClick);
            
            #line default
            #line hidden
            return;
            case 6:
            this.VisualizeStudCountsText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 7:
            this.VisualizeRAMCamber = ((System.Windows.Controls.Button)(target));
            
            #line 92 "..\..\..\Views\AnnotationTypeSelectionForVisualization.xaml"
            this.VisualizeRAMCamber.Click += new System.Windows.RoutedEventHandler(this.OnAnnotationToVisualizeClick);
            
            #line default
            #line hidden
            return;
            case 8:
            this.VisualizeCamberText = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

