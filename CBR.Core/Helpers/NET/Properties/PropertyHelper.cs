using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using CBR.Core.Services;
using System.Reflection;

namespace CBR.Core.Helpers.NET.Properties
{
	public class PropertyHelper
	{
		/// <summary>
		/// return property view models for group properties
		/// </summary>
		/// <param name="objType"></param>
		/// <returns></returns>
		public List<PropertyViewModel> GetGroupViewModelsWithDyn(Type objType)
		{
			List<PropertyViewModel> propertyList = GetGroupViewModels(objType);
			propertyList.AddRange(
				GetDynProperties().Select(p => new PropertyViewModel(p)));
			return propertyList;
		}

		/// <summary>
		/// return property view models for sort properties
		/// </summary>
		/// <param name="objType"></param>
		/// <returns></returns>
		public List<PropertyViewModel> GetSortViewModelsWithDyn(Type objType)
		{
			List<PropertyViewModel> propertyList = GetSortViewModels(objType);
			propertyList.AddRange(
				GetDynProperties().Select(p => new PropertyViewModel(p)));
			return propertyList;
		}

		/// <summary>
		/// return property view models for group properties
		/// </summary>
		/// <param name="objType"></param>
		/// <returns></returns>
		public List<PropertyViewModel> GetGroupViewModels(Type objType)
		{
			List<PropertyModel> propertyList = GetGroupProperties(objType);

			return propertyList.Select(p => new PropertyViewModel(p)).ToList();
		}

		/// <summary>
		/// return property view models for sort properties
		/// </summary>
		/// <param name="objType"></param>
		/// <returns></returns>
		public List<PropertyViewModel> GetSortViewModels(Type objType)
		{
			List<PropertyModel> propertyList = GetSortProperties(objType);

			return propertyList.Select( p=>new PropertyViewModel(p)).ToList();
		}

		/// <summary>
		/// return property models for group properties
		/// </summary>
		/// <param name="objType"></param>
		/// <returns></returns>
		public List<PropertyModel> GetGroupProperties(Type objType)
		{
			List<PropertyModel> propertyList = GetUserProperties(objType);

			return propertyList.Where(p => p.IsGroup).ToList();
		}

		/// <summary>
		/// return property models for sort properties
		/// </summary>
		/// <param name="objType"></param>
		/// <returns></returns>
		public List<PropertyModel> GetSortProperties(Type objType)
		{
			List<PropertyModel> propertyList = GetUserProperties(objType);

			return propertyList.Where(p => p.IsSort).ToList();
		}

		/// <summary>
		/// Extract workspace dynamics as PropertyModel
		/// </summary>
		/// <returns></returns>
		private List<PropertyModel> GetDynProperties()
		{
			return WorkspaceService.Instance.Settings.Dynamics.Select(
				p => new PropertyModel()
				{
					Name = p, IsDynamic = true, IsGroup = true, IsSort = true, LabelKey = p
				}
				).ToList();
		}

		/// <summary>
		/// base funtion that retreive the user property attributes
		/// </summary>
		/// <param name="objType"></param>
		/// <returns></returns>
		private List<PropertyModel> GetUserProperties(Type objType)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("PropertyHelper.GetUserProperties");
			try
			{
				List<PropertyModel> propertyList = new List<PropertyModel>();

				foreach (PropertyInfo prop in objType.GetProperties())
				{
					foreach (Attribute attr in prop.GetCustomAttributes(true))
					{
						if (attr is UserPropertyAttribute)
						{
							UserPropertyAttribute upa = attr as UserPropertyAttribute;
							propertyList.Add(
											new PropertyModel()
											{
												Name = prop.Name,
												LabelKey = upa.LabelKey,
												IsSort = upa.CanSort,
												IsGroup = upa.CanGroup,
												IsDynamic = false
											});
						}
					}
				}

				//foreach (string dyn in WorkspaceService.Instance.Settings.Dynamics)
				//    propertyList.Add(new PropertyModel() { Prefix = "Dynamics.", Name = dyn, LabelKey = dyn });

				return propertyList;
			}
			catch (Exception err)
			{
				LogHelper.Manage("PropertyHelper.GetUserProperties", err);
				return null;
			}
			finally
			{
				LogHelper.End("PropertyHelper.GetUserProperties");
			}  
		}
	}
}
