using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Plugins.Tinject.LifeCircle;

namespace Plugins.Tinject
{
    /// <summary>
    /// The types and instances container.
    /// </summary>
    public class Container
    {
        /// <summary>
        /// Default ID. New ID cannot be same with it.
        /// </summary>
        private const ushort ReservedID = 0;
        
        private readonly MonoInstaller _installer;
        private readonly List<BindRecord> _typeRecords = new List<BindRecord>();
        private readonly List<ToRecord> _instanceRecords = new List<ToRecord>();

        public Container(MonoInstaller installer)
        {
            _installer = installer;
        }

        #region Bind

        /// <summary>
        /// The incoming binding types count.
        /// </summary>
        private int _count;

        /// <summary>
        /// Bind these types into the container.
        /// </summary>
        /// <param name="types"></param>
        /// <returns>The container itself. You don't have to use it under normal conditions.</returns>
        public Container Bind(params Type[] types)
        {
            foreach (var type in types)
            {
                _typeRecords.Add(new BindRecord
                {
                    BindType = type,
                    ID = ReservedID,
                    Instance = null
                });
            }

            _count = types.Length;
            return this;
        }

        /// <summary>
        /// Specify an id to this instance.
        /// </summary>
        /// <param name="id">The id, it has to be greater than 0.</param>
        /// <returns>The container itself. You don't have to use it under normal conditions.</returns>
        /// <exception cref="Exception">Id can't be same with 0.</exception>
        public Container WithId(ushort id)
        {
            if (id == ReservedID)
                throw new Exception(
                    $"Argument id {id} is reserved by the injection container, please specify a number greater than {ReservedID}");

            var startIndex = _typeRecords.Count - _count;
            for (var i = startIndex; i < _typeRecords.Count; i++)
            {
                _typeRecords[i].ID = id;
            }

            return this;
        }

        /// <summary>
        /// The incoming instance type.
        /// </summary>
        private Type _tempType;

        /// <summary>
        /// Specify what type of instance to create.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>The container itself. You don't have to use it under normal conditions.</returns>
        public Container To<T>()
        {
            _tempType = typeof(T);
            _instanceRecords.Add(new ToRecord
            {
                ToType = _tempType
            });
            return this;
        }

        /// <summary>
        /// Specify an instance to the container.
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <exception cref="Exception"></exception>
        public void FromInstance(object instance)
        {
            var startIndex = _typeRecords.Count - _count;
            for (var i = startIndex; i < _typeRecords.Count; i++)
            {
                _typeRecords[i].Instance = instance;
            }

            var instanceRecord = _instanceRecords.FirstOrDefault(r => r.ToType == _tempType);
            if (instanceRecord == null)
                throw new Exception("This should not appear!");

            instanceRecord.Instance = instance;
        }

        /// <summary>
        /// Specify an instance from its constructor with corresponding count of parameters.
        /// </summary>
        /// <param name="parameters">The parameters of the constructor you would use.</param>
        /// <exception cref="Exception">The parameters you specified is not corresponding to the constructor you would use.</exception>
        public void FromNew(params object[] parameters)
        {
            object instance = null;
            if (parameters == null)
            {
                instance = Activator.CreateInstance(_tempType);
            }
            else if (parameters.Length == 0)
            {
                instance = Activator.CreateInstance(_tempType);
            }
            else
            {
                var constructor = _tempType.GetConstructors().FirstOrDefault();
                //default constructor without parameters
                if (constructor == null)
                {
                    instance = Activator.CreateInstance(_tempType);
                }
                else
                {
                    var ctorParameters = constructor.GetParameters();
                    if (ctorParameters.Length != parameters.Length)
                        throw new Exception(
                            $"The count of parameters you specified is not equal to the parameters in the constructor in class {_tempType.FullName}");

                    instance = Activator.CreateInstance(_tempType, parameters);
                }
            }

            var startIndex = _typeRecords.Count - _count;
            for (var i = startIndex; i < _typeRecords.Count; i++)
            {
                _typeRecords[i].Instance = instance;
            }

            var instanceRecord = _instanceRecords.FirstOrDefault(r => r.ToType.FullName == _tempType.FullName);
            if (instanceRecord == null)
                throw new Exception("This should not appear!");

            instanceRecord.Instance = instance;
        }

        /// <summary>
        /// Every bind record
        /// </summary>
        private class BindRecord
        {
            public Type BindType;
            public ushort ID;
            public object Instance;
        }

        /// <summary>
        /// Every instance record
        /// </summary>
        private class ToRecord
        {
            public Type ToType;
            public object Instance;
        }

        #endregion

        #region Injection

        /// <summary>
        /// When finished binding, the container will specify instances to the fields with Inject attribute, and add life circle interface instance to the life circle manager.
        /// </summary>
        public void OnBindFinished()
        {
            foreach (var instanceRecord in _instanceRecords)
            {
                var fields = instanceRecord.ToType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (var fieldInfo in fields)
                {
                    if (fieldInfo.GetCustomAttribute(typeof(InjectAttribute)) is InjectAttribute attr)
                    {
                        if (attr.ID == ReservedID)
                        {
                            fieldInfo.SetValue(instanceRecord.Instance,
                                RetrieveInstance(fieldInfo.FieldType.FullName));
                        }
                        else
                        {
                            fieldInfo.SetValue(instanceRecord.Instance,
                                RetrieveInstance(fieldInfo.FieldType.FullName, attr.ID));
                        }
                    }
                }
            }

            foreach (var typeRecord in _typeRecords)
            {
                if (typeRecord.Instance is ILifeCircle lifeCircle)
                {
                    _installer.AddLifeCircle(lifeCircle, typeRecord.BindType);
                }
            }
        }

        /// <summary>
        /// Retrieve instance with bind type full name.
        /// </summary>
        /// <param name="bindType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private object RetrieveInstance(string bindType)
        {
            var instances = _typeRecords.Where(r => r.BindType.FullName == bindType).ToList();
            if (instances == null)
                throw new Exception($"Cannot find the instance of type {bindType}!");
            var count = instances.Count();
            if (count == 0)
                throw new Exception($"Cannot find the instance of type {bindType}!");
            if (count > 1)
                throw new Exception(
                    $"Found multiple instances of type {bindType}. Perhaps you should use [Inject(Id=id)].");

            return instances.FirstOrDefault()?.Instance;
        }

        /// <summary>
        /// Retrieve instance with bind type full name and id.
        /// </summary>
        /// <param name="bindType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private object RetrieveInstance(string bindType, ushort id)
        {
            var instances = _typeRecords.Where(r => r.BindType.FullName == bindType && r.ID == id).ToList();
            if (instances == null)
                throw new Exception($"Cannot find the instance of type {bindType}!");
            var count = instances.Count();
            if (count == 0)
                throw new Exception($"Cannot find the instance of type {bindType}!");
            if (count > 1)
                throw new Exception(
                    $"Found multiple instances of type {bindType}. Perhaps you should use [Inject(Id=id)].");

            return instances.FirstOrDefault()?.Instance;
        }

        #endregion
    }
}