﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.UnitTest
{
    /// <summary>
    /// NOTE: Variable unknown is set by default as non-deterministic (AnyValue)
    /// </summary>
    [TestClass]
    public class ForwardAnalysisTest
    {
        readonly static TestCase BranchMerge_CASE = @"
$str='f1';
if($unknown){
    $str='f1a';
}else{
    $str='f1b';
}
".AssertVariable("str").HasValues("f1a", "f1b");

        readonly static TestCase BranchMergeWithUndefined_CASE = @"
if($unknown){
    $str='f1a';
}
".AssertVariable("str").HasUndefinedValue().HasUndefinedOrValues("f1a");

        readonly static TestCase UnaryNegation_CASE = @"
$result=42;
$result=-$result;
".AssertVariable("result").HasValues(-42);

        readonly static TestCase NativeCallProcessing_CASE = @"
$call_result=strtolower('TEST');
".AssertVariable("call_result").HasValues("test");

        readonly static TestCase NativeCallProcessing2Arguments_CASE = @"
$call_result=concat('A','B');
".AssertVariable("call_result").HasValues("AB");

        readonly static TestCase NativeCallProcessingNestedCalls_CASE = @"
$call_result=concat(strtolower('Ab'),strtoupper('Cd'));
".AssertVariable("call_result").HasValues("abCD");

        readonly static TestCase IndirectCall_CASE = @"
$call_name='strtolower';
$call_result=$call_name('TEST');
".AssertVariable("call_result").HasValues("test");

        readonly static TestCase BranchedIndirectCall_CASE = @"
if($unknown){
    $call_name='strtolower';
}else{
    $call_name='strtoupper';
}
$call_result=$call_name('TEst');
".AssertVariable("call_result").HasValues("TEST", "test");

        readonly static TestCase MustAliasAssign_CASE = @"
$VarA='ValueA';
$VarB='ValueB';
$VarA=&$VarB;
".AssertVariable("VarA").HasValues("ValueB");

        /// <summary>
        /// This is virtual reference model specific test
        /// </summary>
        readonly static TestCase MayAliasAssign_CASE = @"
$VarA='ValueA';
$VarB='ValueB';
$VarC='ValueC';
if($unknown){
    $VarA=&$VarB;
}else{
    $VarA=&$VarC;
}
$VarA='Assigned';
".AssertVariable("VarA").HasValues("ValueB", "ValueC", "Assigned")
 .AssertVariable("VarB").HasValues("ValueB", "Assigned")
 .AssertVariable("VarC").HasValues("ValueC", "Assigned");

        readonly static TestCase EqualsAssumption_CASE = @"
$Var='init';
if($unknown=='PossibilityA'){
    $Var=$unknown;
}
".AssertVariable("Var").HasValues("init", "PossibilityA");

        readonly static TestCase DynamicEqualsAssumption_CASE = @"
if($unknown){
    $Var='VarA';
}else{
    $Var='VarB';
}

if($unknown){
    $Value='Value1';
}else{
    $Value='Value2';
}

if($$Var==$Value){
    $OutputA=$VarA;
    $OutputB=$VarB;
}
".AssertVariable("OutputA").HasValues("Value1", "Value2")
 .AssertVariable("OutputB").HasValues("Value1", "Value2")
 .SetNonDeterministic("VarA", "VarB");

        readonly static TestCase CallEqualsAssumption_CASE = @"
if($unknown==strtolower(""TestValue"")){
    $Output=$unknown;
}
".AssertVariable("Output").HasValues("testvalue");

        readonly static TestCase ReverseCallEqualsAssumption_CASE = @"
if(abs($unknown)==5){
    $Output=$unknown;
}

".AssertVariable("Output").HasValues(5, -5);


        readonly static TestCase IndirectVarAssign_CASE = @"
$Indirect='x';
$ID='Indirect';
$$ID='Indirectly assigned';
".AssertVariable("Indirect").HasValues("Indirectly assigned");


        readonly static TestCase MergedReturnValue_CASE = @"
function testFunction(){
    if($unknown){
        return 'ValueA';
    }else{
        return 'ValueB';
    }
}

$CallResult=testFunction();
".AssertVariable("CallResult").HasValues("ValueA", "ValueB");

        readonly static TestCase MergedFunctionDeclarations_CASE = @"
if($unknown){
    function testFunction(){
        return 'ValueA';
    }
}else{
    function testFunction(){
        return 'ValueB';
    }
}

$CallResult=testFunction();
".AssertVariable("CallResult").HasValues("ValueA", "ValueB");

        readonly static TestCase ObjectFieldMerge_CASE = @"
class Obj{
    var $a;
}

$obj=new Obj();
if($unknown){
    $obj->a='ValueA';
}else{
    $obj->a='ValueB';
}

$FieldValue=$obj->a;
".AssertVariable("FieldValue").HasValues("ValueA", "ValueB");

        readonly static TestCase ArrayFieldMerge_CASE = @"
if($unknown){
    $arr[0]='ValueA';
}else{
    $arr[0]='ValueB';
}

$ArrayValue=$arr[0];
".AssertVariable("ArrayValue").HasValues("ValueA", "ValueB");

        readonly static TestCase ArrayFieldUpdateMultipleArrays_CASE = @"
if($unknown){
    $arr[0]='ValueA';
}else{
    $arr[0]='ValueB';
}

$arr[0] = 'NewValue';

$ArrayValue=$arr[0];
".AssertVariable("ArrayValue").HasValues("NewValue");


        readonly static TestCase ObjectMethodCallMerge_CASE = @"
class Obj{
    var $a;

    function setter($value){
        $this->a=$value;
    }
}

$obj=new Obj();
if($unknown){
    $obj->setter('ValueA');
}else{
    $obj->setter('ValueB');
}

$FieldValue=$obj->a;
".AssertVariable("FieldValue").HasValues("ValueA", "ValueB");

        readonly static TestCase ObjectMultipleObjectsInVariableRead_CASE = @"
class Cl {
    var $field;
}

if ($unknown) {
    $obj = new Cl();
    $obj->field = 'value';
} else {
    $obj = new Cl();
    $obj->field = 'value';
}
$FieldValue = $obj->field;
".AssertVariable("FieldValue").HasValues("value");

        readonly static TestCase ObjectMultipleObjectsInVariableWrite_CASE = @"
class Cl {
    var $field;
}

if ($unknown) {
    $obj = new Cl();
    $obj->field = 'value';
} else {
    $obj = new Cl();
    $obj->field = 'value';
}
// $obj->field can be strongly updated because the object stored in a variable $obj is not stored in anay other variable
// however, in general case the update must be weak (see ObjectMultipleObjectsInVariableMultipleVariablesWeakWrite_CASE)
$obj->field = 'newValue';
$FieldValue = $obj->field;
"
           // .AssertVariable("FieldValue").HasValues("value", "newValue")
            // more precise implementation would perform strong update:
            .AssertVariable("FieldValue").HasValues("newValue")
            ;

        readonly static TestCase ObjectMultipleObjectsInVariableMultipleVariablesWeakWrite_CASE = @"
class Cl {
    var $field;
}

$a = new Cl();
$a->field = 'value';
$b = new Cl();
$b->field = 'value';
if ($unknown) {
    $obj = $a;
} else {
    $obj = $b;
}
// $a->field and $b->field must be weakly updated
$obj->field = 'newValue';
$FieldValueObj = $obj->field;
$FieldValueA = $a->field;
$FieldValueB = $b->field;
// $a->field must be strongly updated, $obj->field should be weakly updated
$a->field = 'newValue2';
$FieldValueObj2 = $obj->field;
$FieldValueA2 = $a->field;
$FieldValueB2 = $b->field;
"
            .AssertVariable("FieldValueObj").HasValues("value", "newValue")
            .AssertVariable("FieldValueA").HasValues("value", "newValue")
            .AssertVariable("FieldValueB").HasValues("value", "newValue")
            .AssertVariable("FieldValueObj2").HasValues("value", "newValue", "newValue2")
            .AssertVariable("FieldValueA2").HasValues("newValue2")
            .AssertVariable("FieldValueB2").HasValues("value", "newValue")
            ;

        readonly static TestCase ObjectMultipleObjectsInVariableDifferentClassRead_CASE = @"
class ClA {
    var $field;
}
class ClB {
    var $field;
}

if ($unknown) {
    $obj = new ClA();
    $obj->field = 'value1';
} else {
    $obj = new ClB();
    $obj->field = 'value2';
}
$FieldValue = $obj->field;
".AssertVariable("FieldValue").HasValues("value1", "value2");

        readonly static TestCase ObjectMethodObjectSensitivity_CASE = @"
class Cl {
    var $field;
    function f($arg) {$this->field = $arg;}
}
if ($unknown) {
    $obj = new Cl();
    $obj->field = 'originalValue';
}
else {
    $obj = new Cl();
    $obj->field = 'originalValue';
}
// it should call Cl::f() two times - each time for single instance of the class Cl being as $this. Both calls strongly update the value of the field to 'newValue'
$obj->f('newValue');
$FieldValue = $obj->field;
".AssertVariable("FieldValue").HasValues("newValue");

        readonly static TestCase ObjectMethodObjectSensitivityMultipleVariables_CASE = @"
class Cl {
    var $field;
    function f($arg) {$this->field = $arg;}
}
$a = new Cl();
$a->field = 'valueA';
$b = new Cl();
$b->field = 'valueB';
if ($unknown) {
    $obj = $a;
}
else {
    $obj = $b;
}
// it should call Cl::f() two times - each time for single instance of the class Cl being as $this. Both calls strongly update the value of the field to 'newValue'
$obj->f('newValue');
$FieldValueObj = $obj->field;
$FieldValueA = $a->field;
$FieldValueB = $b->field;
$a->f('newValue2');
$FieldValueObj2 = $obj->field;
$FieldValueA2 = $a->field;
$FieldValueB2 = $b->field;
"
            .AssertVariable("FieldValueObj").HasValues("newValue")
            .AssertVariable("FieldValueA").HasValues("newValue")
            .AssertVariable("FieldValueB").HasValues("newValue")
            .AssertVariable("FieldValueObj2").HasValues("newValue", "newValue2")
            .AssertVariable("FieldValueA2").HasValues("newValue2")
            .AssertVariable("FieldValueB2").HasValues("newValue");

        readonly static TestCase ObjectMethodObjectSensitivityDifferentClass_CASE = @"
class ClA {
    var $field = 'valueFromClA';
    function f($arg) {$this->field = $arg;}
}
class ClB {
    var $field = 'valueFromClA';
    function f($arg) {$this->field = $arg;}
}
if ($unknown) {
    $obj = new ClA();
    $obj->field = 'originalValueA';
}
else {
    $obj = new ClB();
    $obj->field = 'originalValueB';
}
// it should call ClA::f() with $this being the instance of ClA() and ClB::f() with $this being the instance of ClB() => it should perform a strong update
$obj->f('newValue');
$FieldValue = $obj->field;
".AssertVariable("FieldValue").HasValues("newValue");


        readonly static TestCase DynamicIncludeMerge_CASE = @"
if($unknown){
    $file='file_a.php';
}else{
    $file='file_b.php';
}

include $file;
".AssertVariable("IncludedVar").HasValues("ValueA", "ValueB")
 .Include("file_a.php", @"
    $IncludedVar='ValueA';
")
 .Include("file_b.php", @"
    $IncludedVar='ValueB';
");


        readonly static TestCase IncludeReturn_CASE = @"
$IncludeResult=(include 'test.php');

".AssertVariable("IncludeResult").HasValues("IncludedReturn")
 .Include("test.php", @"
    return 'IncludedReturn';
");

        readonly static TestCase SimpleXSSDirty_CASE = @"
$x=$_POST['dirty'];
$x=$x;
".AssertVariable("x").IsXSSDirty();


        readonly static TestCase XSSSanitized_CASE = @"
$x=$_POST['dirty'];
$x='sanitized';
".AssertVariable("x").IsXSSClean();

        readonly static TestCase XSSPossibleDirty_CASE = @"
$x=$_POST['dirty'];
if($unknown){
    $x='sanitized';
}
".AssertVariable("x").IsXSSDirty();


        readonly static TestCase ConstantDeclaring_CASE = @"
const test='Direct constant';

if($unknown){
    define('declared','constant1');
}else{
    define('declared','constant2');
}

$x=declared;
$y=test;

".AssertVariable("x").HasValues("constant1", "constant2")
 .AssertVariable("y").HasValues("Direct constant");

        readonly static TestCase BoolResolving_CASE = @"
if($unknown){
    $x=true;
}else{
    $x=false;
}
".AssertVariable("x").HasValues(true, false);

        readonly static TestCase ForeachIteration_CASE = @"
$arr[0]='val1';
$arr[1]='val2';
$arr[2]='val3';

foreach($arr as $value){
    if($unknown ==  $value){
        $test=$value;
    }
}
".AssertVariable("test").HasValues("val1", "val2", "val3");

        readonly static TestCase NativeObjectUsage_CASE = @"
    $obj=new NativeType('TestValue');
    $value=$obj->GetValue();
".AssertVariable("value").HasValues("TestValue")
         .DeclareType(SimpleNativeType.CreateType());


        readonly static TestCase GlobalStatement_CASE = @"

function setGlobal(){
    global $a;
    $a='ValueA';    
}

function setLocal(){
    $a='LocalValueA';
}

setGlobal();
setLocal();

".AssertVariable("a").HasValues("ValueA");

        readonly static TestCase SharedFunction_CASE = @"
function sharedFn($arg){
    return $arg;
}

sharedFn(1);
$resultA=sharedFn(2);

"
 .AssertVariable("resultA").HasValues(1, 2)
 .ShareFunctionGraph("sharedFn")
 ;

        readonly static TestCase SharedFunctionStrongUpdate_CASE = @"
function sharedFn($arg){
    return $arg;
}

function local_wrap() {
    $result[1] = 'InitA';
    $result[2] = 'InitB';
    $result[1]=sharedFn('ValueA');
    $result[2]=sharedFn('ValueB');
    return $result;
}

$resultG = local_wrap();
$resultA = $resultG[1];
$resultB = $resultG[2];

"
 .AssertVariable("resultA").HasValues("ValueA", "ValueB")
 // The following assertion holds but it is not correct
 //.AssertVariable("resultA").HasValues("InitA", "ValueA", "ValueB")
 .AssertVariable("resultB").HasValues("ValueA", "ValueB")
 
 .AssertVariable("resultB").HasValues("ValueA", "ValueB")
 .ShareFunctionGraph("sharedFn")
 ;

        readonly static TestCase SharedFunctionStrongUpdateGlobal_CASE = @"
function sharedFn($arg){
    return $arg;
}

$resultA = 'InitA';
$resultB = 'InitB';
$resultA=sharedFn('ValueA');
$resultB=sharedFn('ValueB');

"

// NOTE: Shared graphs cannot distinct between global contexts in places where theire called
// so the second sharedFn call in second iteration will merge these global contexts 
// {resultA: 'InitA', resultB: 'InitB'} {resultA: 'ValueA','ValueB', resultB: 'ValueA','ValueB'}
// after the merge, resultB assign is processed.
// .AssertVariable("resultA").HasValues("ValueA", "ValueB") This is incorrect because of global contexts cannot be distinguished
 .AssertVariable("resultA").HasValues("InitA", "ValueA", "ValueB")
 .AssertVariable("resultB").HasValues("ValueA", "ValueB")
 .ShareFunctionGraph("sharedFn")
 ;

        readonly static TestCase SharedFunctionWithBranching_CASE = @"
function sharedFn($arg){
    return $arg;
}

$resultA = 'InitA';
$resultB = 'InitB';
if($unknown){
    $resultA=sharedFn('ValueA');
}else{
    $resultB=sharedFn('ValueB');
}

"
         .AssertVariable("resultA").HasValues("InitA", "ValueA", "ValueB")
         .AssertVariable("resultB").HasValues("InitB", "ValueA", "ValueB")
         .ShareFunctionGraph("sharedFn")
         ;

        readonly static TestCase SharedFunctionGlobalVariable_CASE = @"
function sharedFn(){
    global $g;
    return $g;
}

$g = 1;
sharedFn();
$g = 2;
$result = sharedFn();

"
 .AssertVariable("result").HasValues(1, 2)
 .ShareFunctionGraph("sharedFn")
 ;
        // TODO: fails for the same reason as SharedFunctionStrongUpdate_CASE
        readonly static TestCase SharedFunctionAliasing_CASE = @"
function sharedFn($arg){
    $arg = 'fromSharedFunc';
}

function local_wrap() {
    sharedFn(&$a);
    sharedFn(&$b);   

    $res[1] = $a;
    $res[2] = $b;
    return $res;
}

$result = local_wrap();
$a = $result[1];
$b = $result[2];
"
.AssertVariable("a").HasValues("fromSharedFunc")
.AssertVariable("b").HasValues("fromSharedFunc")
.ShareFunctionGraph("sharedFn")
 ;

        readonly static TestCase SharedFunctionAliasingGlobal_CASE = @"
function sharedFn($arg){
    $arg = 'fromSharedFunc';
}

sharedFn(&$a);
sharedFn(&$b);
"
.AssertVariable("a").HasUndefinedValue().HasUndefinedOrValues("fromSharedFunc")
// The following assertion holds but it is strange
//.AssertVariable("a").HasValues("fromSharedFunc")
.AssertVariable("b").HasValues("fromSharedFunc")
// The following assertion holds but it is not correct - the variable $b should be strongly updated 
// the same way as in SharedFunctionStrongUpdateGlobal_CASE
//.AssertVariable("b").HasUndefinedValue().HasUndefinedOrValues("fromSharedFunc")
.ShareFunctionGraph("sharedFn")
 ;
        readonly static TestCase SharedFunctionAliasingGlobal2_CASE = @"
function sharedFn($arg){
    $arg = 'fromSharedFunc';
}

$a = 'initA';
$b = 'initB';
sharedFn(&$a);
sharedFn(&$b);
"
.AssertVariable("a").HasValues("initA", "fromSharedFunc")
.AssertVariable("b").HasValues("fromSharedFunc")
// The following assertion holds but it is not correct - the variable $b should be strongly updated 
// the same way as in SharedFunctionStrongUpdateGlobal_CASE
//.AssertVariable("b").HasValues("initB", "fromSharedFunc")
.ShareFunctionGraph("sharedFn")
 ;

        // TODO: fails for the same reason as SharedFunctionStrongUpdate_CASE
        readonly static TestCase SharedFunctionAliasing2_CASE = @"
function sharedFn($arg){
    $arg = 'fromSharedFunc';
}

function local_wrap() {
    $a = 'originalA';
    $b = 'originalB';
    sharedFn(&$a);
    sharedFn(&$b);

    $res[1] = $a;
    $res[2] = $b;
    return $res;
}

$result = local_wrap();
$a = $result[1];
$b = $result[2];
"
.AssertVariable("a").HasValues("fromSharedFunc")
.AssertVariable("b").HasValues("fromSharedFunc")
.ShareFunctionGraph("sharedFn")
 ;

        // TODO: fails for the same reason as SharedFunctionStrongUpdate_CASE
        readonly static TestCase SharedFunctionAliasingTwoArguments_CASE = @"
function sharedFn($arg, $arg2){
    $arg = $arg2;
}

function local_wrap() {
    sharedFn(&$a, 'fromCallSite1');
    sharedFn(&$b, 'fromCallSite2');

    $res[1] = $a;
    $res[2] = $b;
    return $res;
}

$result = local_wrap();
$a = $result[1];
$b = $result[2];


"
.AssertVariable("a").HasValues("fromCallSite1", "fromCallSite2")
.AssertVariable("b").HasValues("fromCallSite1", "fromCallSite2")
.ShareFunctionGraph("sharedFn")
 ;

        // TODO: fails for the same reason as BranchMergeWithUndefined_CASE
        readonly static TestCase SharedFunctionAliasingMayTwoArguments_CASE = @"
function sharedFn($arg, $arg2){
    $arg = $arg2;
}

function local_wrap() {
    if ($unknown) $c = &$a;
    sharedFn(&$a, 'fromCallSite1');
    sharedFn(&$b, 'fromCallSite2');

    $res[1] = $a;
    $res[2] = $b;
    return $res;
}

$result = local_wrap();
$a = $result[1];
$b = $result[2];
"
.AssertVariable("a").HasValues("fromCallSite1", "fromCallSite2")
.AssertVariable("b").HasValues("fromCallSite1", "fromCallSite2")
.AssertVariable("c").HasUndefinedValue().HasUndefinedOrValues("fromCallSite1", "fromCallSite2")
.ShareFunctionGraph("sharedFn")
 ;

        readonly static TestCase WriteArgument_CASE = @"
$argument=""Value"";
write_argument($argument);

".AssertVariable("argument").HasValues("Value_WrittenInArgument");

        readonly static TestCase IndirectNewEx_CASE = @"
class Obj{
    var $a;

    function setter($value){
        $this->a=$value;
    }
}

$name=""Obj"";
$obj=new $name();

$obj->setter(""Value"");

$result=$obj->a;
".AssertVariable("result").HasValues("Value");

        readonly static TestCase ArgumentWrite_ExplicitAlias_CASE = @"

function setArg($arg){
    $arg=""Set"";
}

$result1=""NotSet"";
$result2=""NotSet"";

setArg($result1);
setArg(&$result2);
"
    .AssertVariable("result1").HasValues("NotSet")
    .AssertVariable("result2").HasValues("Set");

        readonly static TestCase ArgumentWrite_ExplicitAliasToUndefined_CASE = @"

function setArg($arg){
    $arg=""Set"";
}

setArg(&$result);

"
   .AssertVariable("result").HasValues("Set");


        readonly static TestCase ArgumentWrite_ExplicitAliasToUndefinedItem_CASE = @"

function setArg($arg){
    $arg=""Set"";
}


setArg(&$arr[0]);
$result=$arr[0];
"
.AssertVariable("result").HasValues("Set");


        readonly static TestCase StringConcatenation_CASE = @"
$a='A';
$b='B';

$result=$a.$b.'C';
$result.='D';
"
.AssertVariable("result").HasValues("ABCD");

        readonly static TestCase IncrementEval_CASE = @"
$a=3;
$a+=2;
$post_a=$a++;

$b=5;
$pre_b=++$b
"
            .AssertVariable("a").HasValues(6)
            .AssertVariable("post_a").HasValues(5)
            .AssertVariable("b").HasValues(6)
            .AssertVariable("pre_b").HasValues(6)
            ;

        readonly static TestCase DecrementEval_CASE = @"
$a=7;
$a-=2;
$post_a=$a--;

$b=5;
$pre_b=--$b
"
            .AssertVariable("a").HasValues(4)
            .AssertVariable("post_a").HasValues(5)
            .AssertVariable("b").HasValues(4)
            .AssertVariable("pre_b").HasValues(4)
            ;

        readonly static TestCase StringWithExpression_CASE = @"
$a='A';
$result=""Value $a"";
".AssertVariable("result").HasValues("Value A");


        readonly static TestCase LocalExceptionHandling_CASE = @"
$result='Not catched';
try{
    throw new Exception('Test');
}catch(Exception $ex){
    $result='Catched';
}

".AssertVariable("result").HasValues("Catched")
 .DeclareType(SimpleExceptionType.CreateType())
 ;

        readonly static TestCase CrossStackExceptionHandling_CASE = @"
function throwEx(){
    throw new Exception('Test');
}

$result='Not catched';
try{
   throwEx(); 
}catch(Exception $ex){
    $result='Catched';
}

".AssertVariable("result").HasValues("Catched")
.DeclareType(SimpleExceptionType.CreateType())
;
        // This test fails because the framework creates and initializes in CallPoint local variables for all functions that it is 
        // possible to call. That is, both local variables  $arg1 and $arg2 are initialized in CallPoint. These variables then flows 
        // to entry points of both f and g.
        // This problem should be solved by initializing local variables corresponding to function arguments in entry point of the function, not in call point
        readonly static TestCase InitializingArgumentsOfOthersCallees_CASE = @"
function f($arg1) { return $arg1;}
function g($arg2) { 
    return $arg1; // $arg1 should be undefined, not initialized
}
if ($unknown) $func = 'f';
else $func = 'g';
$result = $func(1); // in CallPoint, both f() and g() can be called
".AssertVariable("result").HasUndefinedValue().HasUndefinedOrValues(1)
;
        readonly static TestCase ParametersByAliasGlobal_CASE = @"
function f($arg) {
    $arg = 2; // changes also value of actual parameter
    $b = 3;
    $arg = &$b; // unaliases formal parameter with actual parameter
    $arg = 4; // does not change the value of actual parameter
}
f(&$result);
".AssertVariable("result").HasValues(2)
;

        // The same as ParametersByAliasGlobal_CASE, but passes parameter from local scope.
        readonly static TestCase ParametersByAliasLocal_CASE = @"
function f($arg) {
    $arg = 2; // changes also value of actual parameter
    $b = 3;
    $arg = &$b; // unaliases formal parameter with actual parameter
    $arg = 4; // does not change the value of actual parameter
}
function local_wrap() {
    $a = 1;
    f(&$a);
    return $a;
}
$result = local_wrap();
".AssertVariable("result").HasValues(2)
;

        readonly static TestCase LongLoopWidening_CASE = @"
$test='NotAffected';

$i=0;
while($i<1000){
    ++$i;
}

".AssertVariable("test").HasValues("NotAffected")
 .WideningLimit(20)
;


        [TestMethod]
        public void BranchMerge()
        {
            AnalysisTestUtils.RunTestCase(BranchMerge_CASE);
        }

        [TestMethod]
        public void BranchMergeWithUndefined()
        {
            AnalysisTestUtils.RunTestCase(BranchMergeWithUndefined_CASE);
        }

        [TestMethod]
        public void UnaryNegation()
        {
            AnalysisTestUtils.RunTestCase(UnaryNegation_CASE);
        }

        [TestMethod]
        public void NativeCallProcessing()
        {
            AnalysisTestUtils.RunTestCase(NativeCallProcessing_CASE);
        }

        [TestMethod]
        public void NativeCallProcessing2Arguments()
        {
            AnalysisTestUtils.RunTestCase(NativeCallProcessing2Arguments_CASE);
        }

        [TestMethod]
        public void NativeCallProcessingNestedCalls()
        {
            AnalysisTestUtils.RunTestCase(NativeCallProcessingNestedCalls_CASE);
        }

        [TestMethod]
        public void IndirectCall()
        {
            AnalysisTestUtils.RunTestCase(IndirectCall_CASE);
        }

        [TestMethod]
        public void BranchedIndirectCall()
        {
            AnalysisTestUtils.RunTestCase(BranchedIndirectCall_CASE);
        }

        [TestMethod]
        public void MustAliasAssign()
        {
            AnalysisTestUtils.RunTestCase(MustAliasAssign_CASE);
        }


        [TestMethod]
        public void MayAliasAssign()
        {
            AnalysisTestUtils.RunTestCase(MayAliasAssign_CASE);
        }

        [TestMethod]
        public void EqualsAssumption()
        {
            AnalysisTestUtils.RunTestCase(EqualsAssumption_CASE);
        }

        [TestMethod]
        public void DynamicEqualsAssumption()
        {
            AnalysisTestUtils.RunTestCase(DynamicEqualsAssumption_CASE);
        }

        [TestMethod]
        public void CallEqualsAssumption()
        {
            AnalysisTestUtils.RunTestCase(CallEqualsAssumption_CASE);
        }

        [TestMethod]
        public void ReverseCallEqualsAssumption()
        {
            AnalysisTestUtils.RunTestCase(ReverseCallEqualsAssumption_CASE);
        }

        [TestMethod]
        public void IndirectVarAssign()
        {
            AnalysisTestUtils.RunTestCase(IndirectVarAssign_CASE);
        }

        [TestMethod]
        public void MergedReturnValue()
        {
            AnalysisTestUtils.RunTestCase(MergedReturnValue_CASE);
        }

        [TestMethod]
        public void MergedFunctionDeclarations()
        {
            AnalysisTestUtils.RunTestCase(MergedFunctionDeclarations_CASE);
        }

        [TestMethod]
        public void ObjectFieldMerge()
        {
            AnalysisTestUtils.RunTestCase(ObjectFieldMerge_CASE);
        }

        [TestMethod]
        public void ArrayFieldMerge()
        {
            AnalysisTestUtils.RunTestCase(ArrayFieldMerge_CASE);
        }

        [TestMethod]
        public void ArrayFieldUpdateMultipleArrays()
        {
            AnalysisTestUtils.RunTestCase(ArrayFieldUpdateMultipleArrays_CASE);
        }

        [TestMethod]
        public void ObjectMethodCallMerge()
        {
            AnalysisTestUtils.RunTestCase(ObjectMethodCallMerge_CASE);
        }

        [TestMethod]
        public void ObjectMultipleObjectsInVariableRead()
        {
            AnalysisTestUtils.RunTestCase(ObjectMultipleObjectsInVariableRead_CASE);
        }

        [TestMethod]
        public void ObjectMultipleObjectsInVariableWrite()
        {
            AnalysisTestUtils.RunTestCase(ObjectMultipleObjectsInVariableWrite_CASE);
        }

        [TestMethod]
        public void ObjectMultipleObjectsInVariableMultipleVariablesWeakWrite()
        {
            AnalysisTestUtils.RunTestCase(ObjectMultipleObjectsInVariableMultipleVariablesWeakWrite_CASE);
        }

        [TestMethod]
        public void ObjectMultipleObjectsInVariableDifferentClassRead()
        {
            AnalysisTestUtils.RunTestCase(ObjectMultipleObjectsInVariableDifferentClassRead_CASE);
        }

        [TestMethod]
        public void ObjectMethodObjectSensitivity()
        {
            AnalysisTestUtils.RunTestCase(ObjectMethodObjectSensitivity_CASE);
        }

        [TestMethod]
        public void ObjectMethodObjectSensitivityMultipleVariables()
        {
            AnalysisTestUtils.RunTestCase(ObjectMethodObjectSensitivityMultipleVariables_CASE);
        }

        [TestMethod]
        public void ObjectMethodObjectSensitivityDifferentClass()
        {
            AnalysisTestUtils.RunTestCase(ObjectMethodObjectSensitivityDifferentClass_CASE);
        }

        [TestMethod]
        public void DynamicIncludeMerge()
        {
            AnalysisTestUtils.RunTestCase(DynamicIncludeMerge_CASE);
        }

        [TestMethod]
        public void IncludeReturn()
        {
            AnalysisTestUtils.RunTestCase(IncludeReturn_CASE);
        }

        [TestMethod]
        public void SimpleXSSDirty()
        {
            AnalysisTestUtils.RunTestCase(SimpleXSSDirty_CASE);
        }

        [TestMethod]
        public void XSSSanitized()
        {
            AnalysisTestUtils.RunTestCase(XSSSanitized_CASE);
        }

        [TestMethod]
        public void XSSPossibleDirty()
        {
            AnalysisTestUtils.RunTestCase(XSSPossibleDirty_CASE);
        }

        [TestMethod]
        public void ConstantDeclaring()
        {
            AnalysisTestUtils.RunTestCase(ConstantDeclaring_CASE);
        }

        [TestMethod]
        public void BoolResolving()
        {
            AnalysisTestUtils.RunTestCase(BoolResolving_CASE);
        }

        [TestMethod]
        public void ForeachIteration()
        {
            AnalysisTestUtils.RunTestCase(ForeachIteration_CASE);
        }

        [TestMethod]
        public void NativeObjectUsage()
        {
            AnalysisTestUtils.RunTestCase(NativeObjectUsage_CASE);
        }

        [TestMethod]
        public void GlobalStatement()
        {
            AnalysisTestUtils.RunTestCase(GlobalStatement_CASE);
        }

        [TestMethod]
        public void SharedFunctionWithBranching()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionWithBranching_CASE);
        }

        [TestMethod]
        public void SharedFunction()
        {
            AnalysisTestUtils.RunTestCase(SharedFunction_CASE);
        }

        [TestMethod]
        public void SharedFunctionStrongUpdate()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionStrongUpdate_CASE);
        }

        [TestMethod]
        public void SharedFunctionStrongUpdateGlobal()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionStrongUpdateGlobal_CASE);
        }

        [TestMethod]
        public void SharedFunctionGlobalVariable()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionGlobalVariable_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasing()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasing_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingGlobal()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingGlobal_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingGlobal2()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingGlobal2_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasing2()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasing2_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingTwoArguments()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingTwoArguments_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingMayTwoArguments()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingMayTwoArguments_CASE);
        }

        [TestMethod]
        public void WriteArgument()
        {
            AnalysisTestUtils.RunTestCase(WriteArgument_CASE);
        }

        [TestMethod]
        public void ArgumentWrite_ExplicitAlias()
        {
            AnalysisTestUtils.RunTestCase(ArgumentWrite_ExplicitAlias_CASE);
        }

        [TestMethod]
        public void ArgumentWrite_ExplicitAliasToUndefined()
        {
            AnalysisTestUtils.RunTestCase(ArgumentWrite_ExplicitAliasToUndefined_CASE);
        }

        [TestMethod]
        public void ArgumentWrite_ExplicitAliasToUndefinedItem()
        {
            AnalysisTestUtils.RunTestCase(ArgumentWrite_ExplicitAliasToUndefinedItem_CASE);
        }

        [TestMethod]
        public void IndirectNewEx()
        {
            AnalysisTestUtils.RunTestCase(IndirectNewEx_CASE);
        }

        [TestMethod]
        public void StringConcatenation()
        {
            AnalysisTestUtils.RunTestCase(StringConcatenation_CASE);
        }

        [TestMethod]
        public void IncrementEval()
        {
            AnalysisTestUtils.RunTestCase(IncrementEval_CASE);
        }

        [TestMethod]
        public void DecrementEval()
        {
            AnalysisTestUtils.RunTestCase(DecrementEval_CASE);
        }

        [TestMethod]
        public void StringWithExpression()
        {
            AnalysisTestUtils.RunTestCase(StringWithExpression_CASE);
        }

        [TestMethod]
        public void LocalExceptionHandling()
        {
            AnalysisTestUtils.RunTestCase(LocalExceptionHandling_CASE);
        }

        [TestMethod]
        public void CrossStackExceptionHandling()
        {
            AnalysisTestUtils.RunTestCase(CrossStackExceptionHandling_CASE);
        }

        [TestMethod]
        public void LongLoopWidening()
        {
            AnalysisTestUtils.RunTestCase(LongLoopWidening_CASE);
        }

        #region Function handling tests
        /// <summary>
        /// Tests whether the framework initializes only arguments of called function and not also arguments
        /// of other possible callees.
        /// </summary>
        [TestMethod]
        public void InitializingArgumentsOfOthersCallees()
        {
            AnalysisTestUtils.RunTestCase(InitializingArgumentsOfOthersCallees_CASE);
        }

        [TestMethod]
        public void ParametersByAliasLocal()
        {
            AnalysisTestUtils.RunTestCase(ParametersByAliasLocal_CASE);
        }
        [TestMethod]
        public void ParametersByAliasGlobal()
        {
            AnalysisTestUtils.RunTestCase(ParametersByAliasGlobal_CASE);
        }
        #endregion
    }
}
