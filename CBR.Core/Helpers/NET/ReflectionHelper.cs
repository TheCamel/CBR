using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Input;

namespace CBR.Core.Helpers
{
    static public class ReflectionHelper
    {
		/// <summary>
		///  Create an object based on type
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns></returns>
		static public object CreateInstance(Type objectType)
		{
			try
			{
				return Activator.CreateInstance(objectType);
			}
			catch (Exception err)
			{
				LogHelper.Manage("ReflectionHelper.CreateInstance", err);
				return null;
			}
		}

		/// <summary>
		///  Create an object based on type with parameters
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns></returns>
		static public object CreateInstance(Type objectType, params object[] args)
		{
			try
			{
				return Activator.CreateInstance(objectType, args);
			}
			catch (Exception err)
			{
				LogHelper.Manage("ReflectionHelper.CreateInstance", err);
				return null;
			}
		}

        /// <summary>
        /// Create an object based on typed string
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
		static public object CreateInstance(string objectType)
        {
			try
			{
				return Activator.CreateInstance(Type.GetType(objectType));
			}
			catch (Exception err)
			{
				LogHelper.Manage("ReflectionHelper.CreateInstance", err);
				return null;
			}
        }

		/// <summary>
		///  Create an object based on type with parameters
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns></returns>
		static public object CreateInstance(string objectType, params object[] args)
		{
			try
			{
				return Activator.CreateInstance(Type.GetType(objectType), args);
			}
			catch (Exception err)
			{
				LogHelper.Manage("ReflectionHelper.CreateInstance", err);
				return null;
			}
		}

        /// <summary>
        /// Execute a command on a target object
        /// </summary>
        /// <param name="commandTarget"></param>
        /// <param name="commandName"></param>
        /// <param name="commandParameter"></param>
		static public void ExecuteICommand(object commandTarget, string commandName, object commandParameter)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("ReflectionHelper.");
			try
			{
                if ( commandTarget != null && !string.IsNullOrEmpty(commandName) )
                {
                    //get the target type
                    Type mySelf = commandTarget.GetType();

                    //get the command as property
                    PropertyInfo pInfo = mySelf.GetProperty(commandName);

                    //retreive the real value as ICommand
                    ICommand com = (ICommand)pInfo.GetValue(commandTarget, null);

                    //Execute the command
                    com.Execute(commandParameter);
                }
			}
			catch (Exception err)
			{
				LogHelper.Manage("ReflectionHelper.", err);
			}
			finally
			{
				LogHelper.End("ReflectionHelper.");
			}  
        }
    }
}
