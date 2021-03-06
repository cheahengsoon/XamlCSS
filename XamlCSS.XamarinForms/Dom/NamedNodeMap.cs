﻿using AngleSharp.Dom;
using XamlCSS.Dom;
using Xamarin.Forms;

namespace XamlCSS.XamarinForms.Dom
{
	public class NamedNodeMap : NamedNodeMapBase<BindableObject, BindableProperty>
	{
		public NamedNodeMap(BindableObject BindableObject)
			: base(BindableObject)
		{

		}

        protected override IAttr CreateAttribute(BindableObject dependencyObject, DependencyPropertyInfo<BindableProperty> propertyInfo)
        {
            return new ElementAttribute(dependencyObject, propertyInfo.Property);
        }
	}
}