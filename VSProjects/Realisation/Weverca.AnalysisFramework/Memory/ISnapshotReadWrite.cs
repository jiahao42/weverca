﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Memory
{
    public interface ISnapshotReadWrite : ISnapshotReadonly
    {
        #region Snapshot flow operations
        /// <summary>
        /// Snapshot has to contain merged info present in inputs (no matter what snapshots contains till now)
        /// This merged info can be than changed with snapshot updatable operations
        /// NOTE: Further changes of inputs can't change extended snapshot
        /// </summary>
        /// <param name="inputs">Input snapshots that should be merged</param>
        void Extend(params ISnapshotReadonly[] inputs);

        /// <summary>
        /// Merge given call output with current context.
        /// WARNING: Call can change many objects via references (they don't has to be in global context)
        /// </summary>
        /// <param name="callOutputs">Output snapshot of call</param>    
        void MergeWithCallLevel(params ISnapshotReadonly[] callOutputs);

        #endregion

        #region Creating values


        //=========TODO==============================
        //=These properties should be rewritten to methods 
        //=(because of storing info for values we don't want any singletons)
        //===========================================

        AnyValue AnyValue { get; }
        UndefinedValue UndefinedValue { get; }
        AnyStringValue AnyStringValue { get; }
        AnyBooleanValue AnyBooleanValue { get; }
        AnyIntegerValue AnyIntegerValue { get; }
        AnyLongintValue AnyLongintValue { get; }
        AnyObjectValue AnyObjectValue { get; }
        AnyArrayValue AnyArrayValue { get; }
        //===========================================


        /// <summary>
        /// Create interval of integers
        /// </summary>
        /// <param name="start">Lower bound</param>
        /// <param name="end">Upper bound</param>
        /// <returns>Created interval</returns>
        IntegerIntervalValue CreateIntegerInterval(int start, int end);

        /// <summary>
        /// Create interval of long integers
        /// </summary>
        /// <param name="start">Lower bound</param>
        /// <param name="end">Upper bound</param>
        /// <returns>Created interval</returns>
        LongintIntervalValue CreateLongintInterval(long start, long end);


        /// <summary>
        /// Create interval of doubles
        /// </summary>
        /// <param name="start">Lower bound</param>
        /// <param name="end">Upper bound</param>
        /// <returns>Created interval</returns>
        FloatIntervalValue CreateFloatInterval(double start, double end);

        /// <summary>
        /// Create string value from given literal
        /// </summary>
        /// <param name="literal">String literal</param>
        /// <returns>Created value</returns>
        StringValue CreateString(string literal);
        
        /// <summary>
        /// Create integer value from given number
        /// </summary>
        /// <param name="number">Represented number</param>
        /// <returns>Created value</returns>
        IntegerValue CreateInt(int number);

        /// <summary>
        /// Create long integer value from given number
        /// </summary>
        /// <param name="number">Represented number</param>
        /// <returns>Created value</returns>
        LongintValue CreateLong(long number);

        /// <summary>
        /// Create float value from given number
        /// </summary>
        /// <param name="number">Represented number</param>
        /// <returns>Created value</returns>
        FloatValue CreateDouble(double number);

        /// <summary>
        /// Create boolean value from given value
        /// </summary>
        /// <param name="boolean">Boolean value</param>
        /// <returns>Created value</returns>
        BooleanValue CreateBool(bool boolean);
        
        /// <summary>
        /// Create info value from given data
        /// NOTE:
        ///     T should provide immutability that avoid wrong usage
        /// </summary>
        /// <typeparam name="T">Type of stored info</typeparam>
        /// <param name="data">Info data</param>
        /// <returns>Created info value</returns>
        InfoValue<T> CreateInfo<T>(T data);

        /// <summary>
        /// Create function value from given declaration
        /// </summary>
        /// <param name="declaration">Function declaration</param>
        /// <returns>Created value</returns>
        FunctionValue CreateFunction(FunctionDecl declaration);

        /// <summary>
        /// Create function value from given declaration
        /// </summary>
        /// <param name="declaration">Method declaration</param>
        /// <returns>Created value</returns>
        FunctionValue CreateFunction(MethodDecl declaration);

        /// <summary>
        /// Create function value from given declaration
        /// </summary>
        /// <param name="analyzer">Analyzer declaration</param>
        /// <param name="name">Name of created analyzer</param>
        /// <returns>Created value</returns>
        FunctionValue CreateFunction(Name name,NativeAnalyzer analyzer);

        /// <summary>
        /// Create type value from given declaration
        /// </summary>
        /// <param name="declaration">Native type declaration</param>
        /// <returns>Created value</returns>
        TypeValueBase CreateType(ClassDecl declaration);        

        /// <summary>
        /// Create function value from given expression
        /// </summary>
        /// <param name="expression">Lambda function declaration</param>        
        /// <returns>Created value</returns>
        FunctionValue CreateFunction(LambdaFunctionExpr expression);

        /// <summary>
        /// Create array empty array
        /// </summary>
        /// <returns>Created value</returns>
        AssociativeArray CreateArray();

        /// <summary>
        /// Create object of given type
        /// </summary>   
        /// <param name="type">Desired type of created object</param>
        ObjectValue CreateObject(TypeValueBase type);

        #endregion

        #region Snapshot entry manipulation

        /// <summary>
        /// Create snapshot entry providing reading,... services for variable
        /// </summary>
        /// <remarks>
        /// If global context is not forced, searches in local context (there can be 
        /// fetched some variables from global context also),
        /// or in global context in snapshot belonging to global code
        /// </remarks>
        /// <param name="name">Name of variable</param>
        /// <param name="forceGlobalContext">Determine that searching in global context has to be forced</param>
        /// <returns>Readable snapshot entry for variable identifier</returns>
        ReadWriteSnapshotEntryBase GetVariable(VariableIdentifier variable, bool forceGlobalContext = false);

        #endregion


        #region Value storing

        /// <summary>
        /// Assign value into targetVar
        /// If value is AliasValue - must alias has to be set for variable
        /// </summary>
        /// <param name="targetVar">Target of assigning</param>
        /// <param name="value">Value that will be assigned</param>
        void Assign(VariableName targetVar, Value value);


        void AssignAliases(VariableName targetVar, IEnumerable<AliasValue> alias);

        /// <summary>
        /// Assign memory entry into targetVar        
        /// </summary>
        /// <param name="targetVar">Target of assigning</param>
        /// <param name="entry">Value that will be assigned</param>
        void Assign(VariableName targetVar, MemoryEntry entry);

        /// <summary>
        /// Set given info for value
        /// </summary>
        /// <param name="value">Value which info is stored</param>
        /// <param name="info">Info stored for value</param>
        void SetInfo(Value value, params InfoValue[] info);

        /// <summary>
        /// Set given info for variable
        /// </summary>
        /// <param name="variable">Variable which info is stored</param>
        /// <param name="info">Info stored for variable</param>
        void SetInfo(VariableName variable, params InfoValue[] info);

        /// <summary>
        /// Set given value into field in objectValue
        /// </summary>
        /// <param name="objectValue">Object which field will be set</param>
        /// <param name="field">Field where value will be stored</param>
        /// <param name="value">Value that will be stored</param>
        void SetField(ObjectValue objectValue, ContainerIndex field, MemoryEntry value);

        /// <summary>
        /// Set given alias into field in objectValue
        /// </summary>
        /// <param name="objectValue">Object which field will be set</param>
        /// <param name="field">Field where alias will be set</param>
        /// <param name="alias">Alias that will be set</param>
        void SetFieldAlias(ObjectValue objectValue, ContainerIndex field, IEnumerable<AliasValue> alias);
        
        /// <summary>
        /// Set given value at specified index in array
        /// </summary>
        /// <param name="array">Array which index will be set</param>
        /// <param name="index">Index where value will be stored</param>
        /// <param name="value">Value that will be stored</param>
        void SetIndex(AssociativeArray array, ContainerIndex index, MemoryEntry value);

        /// <summary>
        /// Set given alias at index in array
        /// </summary>
        /// <param name="array">Array which index will be set</param>
        /// <param name="index">Index where alias will be set</param>
        /// <param name="alias">Alias that will be set</param>
        void SetIndexAlias(AssociativeArray array, ContainerIndex index, IEnumerable<AliasValue> alias);


        #endregion


        #region Global context manipulation

        /// <summary>
        /// Declare function into global scope
        /// </summary>
        /// <param name="declaration">Function declaration</param>
        void DeclareGlobal(FunctionDecl declaration);

        /// <summary>
        /// Declare type into global scope
        /// </summary>
        /// <param name="type">Declared type</param>
        void DeclareGlobal(TypeValueBase type);

        /// <summary>
        /// Fetch variables from global context into current context
        /// </summary>
        /// <example>global x,y;</example>
        /// <param name="variables">Variables that will be fetched</param>
        void FetchFromGlobal(params VariableName[] variables);

        /// <summary>
        /// Fetch all variables defined in global context into current context
        /// </summary>
        void FetchFromGlobalAll();

        #endregion

    }
}