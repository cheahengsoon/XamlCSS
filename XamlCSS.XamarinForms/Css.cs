﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamlCSS.Dom;
using XamlCSS.Utils;
using XamlCSS.Windows.Media;
using XamlCSS.XamarinForms.CssParsing;
using XamlCSS.XamarinForms.Dom;
using XamlCSS.XamarinForms.Internals;

namespace XamlCSS.XamarinForms
{
    public class Css
    {
        public static BaseCss<BindableObject, BindableObject, Style, BindableProperty> instance;

        private static Timer uiTimer;

        private static Element rootElement;
        private static object lockObject = new object();
        private static bool initialized = false;

        private static void StartUiTimer()
        {
            uiTimer = new Timer(TimeSpan.FromMilliseconds(16), (state) =>
            {
                var tcs = new TaskCompletionSource<object>();
                Device.BeginInvokeOnMainThread(() =>
                {
                    instance?.ExecuteApplyStyles();
                    tcs.SetResult(0);
                });
                tcs.Task.Wait();
            }, null);
        }

        public static void Reset()
        {
            lock (lockObject)
            {
                if (initialized == false)
                {
                    return;
                }

                VisualTreeHelper.SubTreeAdded -= VisualTreeHelper_ChildAdded;
                VisualTreeHelper.SubTreeRemoved -= VisualTreeHelper_ChildRemoved;

                VisualTreeHelper.Reset();

                uiTimer?.Cancel();
                uiTimer?.Dispose();
                uiTimer = null;

                instance = null;

                initialized = false;
            }
        }

        public static void Initialize(Element rootElement, Assembly[] resourceSearchAssemblies = null)
        {
            lock (lockObject)
            {
                if (initialized &&
                    rootElement == Css.rootElement)
                {
                    return;
                }

                Reset();

                var markupExtensionParser = new MarkupExtensionParser();
                var dependencyPropertyService = new DependencyPropertyService();
                var cssTypeHelper = new CssTypeHelper<BindableObject, BindableObject, BindableProperty, Style>(markupExtensionParser, dependencyPropertyService);
                
                instance = new BaseCss<BindableObject, BindableObject, Style, BindableProperty>(
                    dependencyPropertyService,
                    new LogicalTreeNodeProvider(dependencyPropertyService),
                    new StyleResourceService(),
                    new StyleService(dependencyPropertyService, markupExtensionParser),
                    DomElementBase<BindableObject, Element>.GetPrefix(typeof(Button)),
                    markupExtensionParser,
                    Device.BeginInvokeOnMainThread,
                    new CssFileProvider(resourceSearchAssemblies ?? new Assembly[0], cssTypeHelper)
                    );

                Css.rootElement = rootElement;

                VisualTreeHelper.SubTreeAdded += VisualTreeHelper_ChildAdded;
                VisualTreeHelper.SubTreeRemoved += VisualTreeHelper_ChildRemoved;

                VisualTreeHelper.Initialize(rootElement);

                if (rootElement is Application)
                {
                    var application = rootElement as Application;

                    if (application.MainPage == null)
                    {
                        PropertyChangedEventHandler handler = null;
                        handler = (s, e) =>
                        {
                            if (e.PropertyName == nameof(Application.MainPage))
                            {
                                application.PropertyChanged -= handler;
                                VisualTreeHelper.Include(application.MainPage);
                            }
                        };

                        application.PropertyChanged += handler;
                    }
                }

                StartUiTimer();

                initialized = true;
            }
        }
        
        public static readonly BindableProperty MatchingStylesProperty =
            BindableProperty.CreateAttached(
                "MatchingStyles",
                typeof(string[]),
                typeof(Css),
                null,
                BindingMode.TwoWay);
        public static string[] GetMatchingStyles(BindableObject obj)
        {
            return obj.GetValue(MatchingStylesProperty) as string[];
        }
        public static void SetMatchingStyles(BindableObject obj, string[] value)
        {
            obj.SetValue(MatchingStylesProperty, value);
        }

        public static readonly BindableProperty AppliedMatchingStylesProperty =
            BindableProperty.CreateAttached(
                "AppliedMatchingStyles",
                typeof(string[]),
                typeof(Css),
                null,
                BindingMode.TwoWay);
        public static string[] GetAppliedMatchingStyles(BindableObject obj)
        {
            return obj.GetValue(AppliedMatchingStylesProperty) as string[];
        }
        public static void SetAppliedMatchingStyles(BindableObject obj, string[] value)
        {
            obj.SetValue(AppliedMatchingStylesProperty, value);
        }

        public static readonly BindableProperty IdProperty =
            BindableProperty.CreateAttached(
                "Id",
                typeof(string),
                typeof(Css),
                null,
                BindingMode.TwoWay);
        public static string GetId(BindableObject obj)
        {
            return obj.GetValue(IdProperty) as string;
        }
        public static void SetId(BindableObject obj, string value)
        {
            obj.SetValue(IdProperty, value);
        }

        public static readonly BindableProperty InitialStyleProperty =
            BindableProperty.CreateAttached(
                "InitialStyle",
                typeof(Style),
                typeof(Css),
                null,
                BindingMode.TwoWay);
        public static Style GetInitialStyle(BindableObject obj)
        {
            return obj.GetValue(InitialStyleProperty) as Style;
        }
        public static void SetInitialStyle(BindableObject obj, Style value)
        {
            obj.SetValue(InitialStyleProperty, value);
        }

        public static readonly BindableProperty HadStyleProperty =
            BindableProperty.CreateAttached(
                "HadStyle",
                typeof(bool?),
                typeof(Css),
                null,
                BindingMode.TwoWay);
        public static bool? GetHadStyle(BindableObject obj)
        {
            return obj.GetValue(HadStyleProperty) as bool?;
        }
        public static void SetHadStyle(BindableObject obj, bool? value)
        {
            obj.SetValue(HadStyleProperty, value);
        }

        public static readonly BindableProperty StyleProperty =
            BindableProperty.CreateAttached(
                "Style",
                typeof(StyleDeclarationBlock),
                typeof(Css),
                null,
                BindingMode.TwoWay,
                null,
                Css.StylePropertyAttached);
        public static StyleDeclarationBlock GetStyle(BindableObject obj)
        {
            return obj.GetValue(StyleProperty) as StyleDeclarationBlock;
        }
        public static void SetStyle(BindableObject obj, StyleDeclarationBlock value)
        {
            obj.SetValue(StyleProperty, value);
        }

        public static readonly BindableProperty StyleSheetProperty =
            BindableProperty.CreateAttached(
                "StyleSheet",
                typeof(StyleSheet),
                typeof(Css),
                null,
                BindingMode.TwoWay,
                null,
                Css.StyleSheetPropertyChanged
                );
        public static StyleSheet GetStyleSheet(BindableObject obj)
        {
            return obj.GetValue(StyleSheetProperty) as StyleSheet;
        }
        public static void SetStyleSheet(BindableObject obj, StyleSheet value)
        {
            obj.SetValue(StyleSheetProperty, value);
        }

        public static readonly BindableProperty ClassProperty =
            BindableProperty.CreateAttached(
                "Class",
                typeof(string),
                typeof(Css),
                null,
                BindingMode.TwoWay,
                null,
                ClassPropertyChanged);
        private static void ClassPropertyChanged(BindableObject element, object oldValue, object newValue)
        {
            var domElement = GetDomElement(element) as DomElementBase<BindableObject, BindableProperty>;
            domElement?.ResetClassList();

            Css.instance?.UpdateElement(element);
        }
        public static string GetClass(BindableObject obj)
        {
            return obj.GetValue(ClassProperty) as string;
        }
        public static void SetClass(BindableObject obj, string value)
        {
            obj.SetValue(ClassProperty, value);
        }

        public static readonly BindableProperty HandledCssProperty =
            BindableProperty.CreateAttached(
                "HandledCss",
                typeof(bool),
                typeof(Css),
                false,
                BindingMode.TwoWay);
        public static bool GetHandledCss(BindableObject obj)
        {
            return ((bool?)obj.GetValue(HandledCssProperty) ?? false);
        }
        public static void SetHandledCss(BindableObject obj, bool value)
        {
            obj.SetValue(HandledCssProperty, value);
        }

        public static readonly BindableProperty DomElementProperty =
            BindableProperty.CreateAttached(
                "DomElement",
                typeof(IDomElement<BindableObject>),
                typeof(Css),
                null,
                BindingMode.TwoWay);
        public static IDomElement<BindableObject> GetDomElement(BindableObject obj)
        {
            return obj?.GetValue(DomElementProperty) as IDomElement<BindableObject>;
        }
        public static void SetDomElement(BindableObject obj, IDomElement<BindableObject> value)
        {
            obj?.SetValue(DomElementProperty, value);
        }

        private static void VisualTreeHelper_ChildAdded(object sender, EventArgs e)
        {
            //Debug.WriteLine("A");
            instance?.UpdateElement(sender as BindableObject);
        }
        private static void VisualTreeHelper_ChildRemoved(object sender, EventArgs e)
        {
            instance?.UnapplyMatchingStyles(sender as Element, null);
        }

        private static void StyleSheetPropertyChanged(BindableObject bindableObject, object oldValue, object newValue)
        {
            var element = bindableObject as Element;

            if (oldValue != null)
            {
                var oldStyleSheet = oldValue as StyleSheet;
                oldStyleSheet.PropertyChanged -= StyleSheet_PropertyChanged;
                //oldStyleSheet.AttachedTo = null;

                instance?.EnqueueRemoveStyleSheet(element, (StyleSheet)oldValue);
            }

            var newStyleSheet = (StyleSheet)newValue;

            if (newStyleSheet == null)
            {
                return;
            }

            newStyleSheet.PropertyChanged += StyleSheet_PropertyChanged;
            newStyleSheet.AttachedTo = element;

            instance?.EnqueueRenderStyleSheet(element, newStyleSheet);
        }

        private static void StyleSheet_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StyleSheet.Content))
            {
                var styleSheet = sender as StyleSheet;
                var attachedTo = styleSheet.AttachedTo as Element;

                instance?.EnqueueUpdateStyleSheet(attachedTo, styleSheet);
            }
        }

        private static void StylePropertyAttached(BindableObject d, object oldValue, object newValue)
        {
            instance?.UpdateElement(d as Element);
        }
    }
}
