﻿using System;
using System.Linq;
using Windows.UI.Xaml;

namespace XamlCSS.UWP
{
	public class DependencyPropertyService : IDependencyPropertyService<DependencyObject, FrameworkElement, Style, DependencyProperty>
	{
		public DependencyProperty GetBindableProperty(DependencyObject frameworkElement, string propertyName)
		{
			return GetBindableProperty(frameworkElement.GetType(), propertyName);
		}
		public DependencyProperty GetBindableProperty(Type bindableObjectType, string propertyName)
		{
			string dpName = propertyName + "Property";
			var dpProperties = TypeHelpers.DeclaredProperties(bindableObjectType);
			var dpProperty = dpProperties.FirstOrDefault(i => i.Name == dpName);

			if (dpProperty != null)
				return dpProperty.GetValue(null) as DependencyProperty;

			var dpFields = TypeHelpers.DeclaredFields(bindableObjectType);
			var dpField = dpFields.FirstOrDefault(i => i.Name == dpName);
			return null;
		}

		public object GetBindablePropertyValue(Type frameworkElementType, DependencyProperty property, object propertyValue)
		{
			return propertyValue;
		}

		public string[] GetAppliedMatchingStyles(DependencyObject obj)
		{
			return Css.GetAppliedMatchingStyles(obj) as string[];
		}

		public string GetClass(DependencyObject obj)
		{
			return Css.GetClass(obj) as string;
		}

		public bool? GetHadStyle(DependencyObject obj)
		{
			return Css.GetHadStyle(obj) as bool?;
		}

		public Style GetInitialStyle(DependencyObject obj)
		{
			return Css.GetInitialStyle(obj) as Style;
		}

		public bool IsLoaded(FrameworkElement obj)
		{
			return Css.GetIsLoaded(obj);
		}

		public string[] GetMatchingStyles(DependencyObject obj)
		{
			return Css.GetMatchingStyles(obj) as string[];
		}

		public string GetName(DependencyObject obj)
		{
			return (obj as FrameworkElement)?.Name;
		}

		public StyleDeclarationBlock GetStyle(DependencyObject obj)
		{
			return Css.GetStyle(obj) as StyleDeclarationBlock;
		}

		public StyleSheet GetStyleSheet(DependencyObject obj)
		{
			return Css.GetStyleSheet(obj) as StyleSheet;
		}

		public void SetAppliedMatchingStyles(DependencyObject obj, string[] value)
		{
			Css.SetAppliedMatchingStyles(obj, value);
		}

		public void SetClass(DependencyObject obj, string value)
		{
			Css.SetClass(obj, value);
		}

		public void SetHadStyle(DependencyObject obj, bool? value)
		{
			Css.SetHadStyle(obj, value);
		}

		public void SetInitialStyle(DependencyObject obj, Style value)
		{
			Css.SetInitialStyle(obj, value);
		}

		public void SetIsLoaded(FrameworkElement obj, bool value)
		{
			Css.SetIsLoaded(obj, value);
		}

		public void SetMatchingStyles(DependencyObject obj, string[] value)
		{
			Css.SetMatchingStyles(obj, value);
		}

		public void SetName(DependencyObject obj, string value)
		{
			(obj as FrameworkElement).Name = value;
		}

		public void SetStyle(DependencyObject obj, StyleDeclarationBlock value)
		{
			Css.SetStyle(obj, value);
		}

		public void SetStyleSheet(DependencyObject obj, StyleSheet value)
		{
			Css.SetStyleSheet(obj, value);
		}

		public void RegisterLoadedOnce(FrameworkElement frameworkElement, Action<object> func)
		{
			RoutedEventHandler handler = null;
			handler = (s, e) =>
			{
				frameworkElement.Loaded -= handler;
				SetIsLoaded(frameworkElement, true);
				func(s);
			};
			frameworkElement.Loaded += handler;
		}
	}
}