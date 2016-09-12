namespace Validation.Fody.Tests.DataAttributes
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public sealed class TypesAttribute : CombinatorialValuesAttribute
    {
        /// <summary>
        /// Various types to try to be as representative of the various kinds of non-ref/non-ptr types as possible
        /// </summary>
        internal static Type[] Types { get; } = {
            typeof(int), // built-in value type
            typeof(short), // built-in value type
            typeof(char), // built-in value type
            typeof(DateTime), // built-in value type
            typeof(decimal), // larger value type
            typeof(bool), // built-in value type
            typeof(float), // built-in floating point value type
            typeof(double), // built-in floating point value type
            typeof(int?), // built-in nullable value type
            typeof(short?), // built-in nullable value type
            typeof(char?), // built-in nullable value type
            typeof(DateTime?), // built-in nullable value type
            typeof(decimal?), // larger nullable value type
            typeof(bool?), // built-in nullable value type
            typeof(float?), // built-in nullable floating point value type
            typeof(double?), // built-in nullable floating point value type
            typeof(string), // reference type
            typeof(BindingFlags), // enum
            typeof(IEnumerable), // interface
            typeof(IEnumerable<int>), // generic interface with value type argument
            typeof(IEnumerable<int?>), // generic interface with nullable value type argument
            typeof(IEnumerable<string>), // generic interface with reference type argument
            typeof(List<short>), // generic class with value type argument
            typeof(List<short?>), // generic class with nullable value type argument
            typeof(List<Random>), // generic class with reference type argument
            typeof(Attribute), // attribute base
            typeof(ObsoleteAttribute), // attribute
            typeof(Exception), // exception base-class
            typeof(InvalidOperationException), // exception
            typeof(Delegate), // delegate base type
            typeof(Action), // delegate type
            typeof(Func<int>), // generic delegate with value type argument
            typeof(Func<int?>), // generic delegate with nullable value type argument
            typeof(Func<string>), // generic delegate with reference type argument
            typeof(char[]), // array with value-type elements
            typeof(char?[]), // array with nullable value-type elements
            typeof(string[]), // array with reference-type elements
            typeof(Array), // non-specific array
            typeof(Type), // type type
        };
        
        public TypesAttribute(params Type[] except) : base(Types.Except(except).ToArray())
        {
        }
    }
}