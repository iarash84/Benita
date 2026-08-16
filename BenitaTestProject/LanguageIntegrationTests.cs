using Benita;

namespace BenitaTestProject
{
    /// <summary>سناریوهای چندلایهٔ زبان را از source تا خروجی runtime اعتبارسنجی می‌کند.</summary>
    [TestClass]
    public class LanguageIntegrationTests
    {
        private CompilerClass _compiler;

        [TestInitialize]
        public void Setup()
        {
            _compiler = new CompilerClass();
        }

        [TestMethod]
        public void NumericAdditionExpressionExecutesCorrectly()
        {
            string source = @"
func myFunction(number a, number b) -> number {
    return a + b;
}
// this is a comment
_main_() {
    number x = 10;
    number y = 8;
    print(x + y);
}";

            _compiler.Check(source);


            var expectedOutput = "18\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [DataTestMethod]
        [DataRow("number[]")]
        [DataRow("string[]")]
        [DataRow("bool[]")]
        public void UninitializedArray_UsesAnEmptyRuntimeArray(string arrayType)
        {
            string source = $"_main_() {{ {arrayType} values; print(array_len(values)); }}";
            using var consoleOutput = new ConsoleOutput();

            _compiler.Exec(source);

            Assert.AreEqual($"0{Environment.NewLine}", consoleOutput.GetOutput());
        }

        [DataTestMethod]
        [DataRow("number", "0")]
        [DataRow("string", "")]
        [DataRow("bool", "False")]
        public void SizedArray_UsesTheElementTypeDefaultValue(string elementType, string expectedValue)
        {
            string source = $"_main_() {{ number size = 2; {elementType}[] values = {elementType}[size]; print(array_len(values)); print(values[0]); }}";
            using var consoleOutput = new ConsoleOutput();

            _compiler.Exec(source, optimizeAst: true);

            Assert.AreEqual($"2{Environment.NewLine}{expectedValue}{Environment.NewLine}",
                consoleOutput.GetOutput());
        }

        [DataTestMethod]
        [DataRow("-1")]
        [DataRow("1.5")]
        public void SizedArray_WithInvalidRuntimeLength_IsRejected(string length)
        {
            var exception = Assert.ThrowsException<RuntimeException>(() =>
                _compiler.Exec($"_main_() {{ number[] values = number[{length}]; }}"));

            StringAssert.Contains(exception.Message, "non-negative whole number");
        }

        [TestMethod]
        public void ToString_AcceptsEveryScalarTypeDeclaredByTheLanguage()
        {
            const string source = """
                _main_() {
                    print(to_string(12));
                    print(to_string(true));
                    print(to_string("Benita"));
                }
                """;
            using var consoleOutput = new ConsoleOutput();

            _compiler.Check(source);
            _compiler.Exec(source);

            Assert.AreEqual($"12{Environment.NewLine}True{Environment.NewLine}Benita{Environment.NewLine}",
                consoleOutput.GetOutput());
        }

        [TestMethod]
        public void GlobalInitializer_CanCallFunctionDeclaredLater()
        {
            const string source = """
                number initial = value();
                func value() -> number { return 7; }
                _main_() { print(initial); }
                """;

            foreach (bool optimize in new[] { false, true })
            {
                using var consoleOutput = new ConsoleOutput();
                _compiler.Check(source);
                _compiler.Exec(source, optimizeAst: optimize);
                Assert.AreEqual($"7{Environment.NewLine}", consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void WhileLoopWithIncrementExecutesCorrectly()
        {
            string source = @"
func myFunction(number a, number b) -> number {
    return a + b;
}
// this is a comment
_main_() {
    number x = 10;
    number y = 20;
    print(x + y);
    number i = 0;
    while ( i < 5 ) {
        x++;
        i++;
    }
    print(x);
}";
            _compiler.Check(source);

            var expectedOutput = "30\r\n15\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }

        }

        [TestMethod]
        public void NumericFunctionCallExecutesCorrectly()
        {
            string source = @"
func add(number a, number b) -> number {
    return a + b;
}

_main_() {
    number result = add(2, 3);
    print(result);
}";
            _compiler.Check(source);

            var expectedOutput = "5\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void NumericExpressionAssignmentExecutesCorrectly()
        {
            string source = @" 
_main_()  {
    number result = 2 + 3;
    print(result);
}";

            _compiler.Check(source);

            var expectedOutput = "5\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void NumericAndStringFunctionsExecuteCorrectly()
        {
            string source = @"
func add(number a, number b) -> number {
    return a + b;
}

func concatenate(string a, string b) -> string {
    return a + b;
}

_main_() {
    number x = 10;
    number y = 20;
    number sum = add(x, y);              
    string message = concatenate(""hello"", "" world"");    
    print(message);
}";

            _compiler.Check(source);

            var expectedOutput = "hello world\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void InputFunctionReadsAndPrintsUserInput()
        {
            string source = @"
_main_() {
    print(""Enter your name "");
    string message = input();
    print(""your name is "");
    print(message);
}";

            _compiler.Check(source);

            var expectedInput = "Tom";
            var expectedOutput = "Enter your name \r\nyour name is \r\nTom\r\n";
            using (var consoleInput = new ConsoleInput(expectedInput))
            {
                using (var consoleOutput = new ConsoleOutput())
                {
                    _compiler.Exec(source);
                    Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
                }
            }
        }

        [TestMethod]
        public void CombinedFunctionsConditionLoopAndInputExecuteCorrectly()
        {
            string source = @"
func add(number a, number b) -> number {
    return a + b;
}

func concatenate(string a, string b) -> string {
    return a + b;
}

func is_even(number a) -> bool {
    return a % 2 == 0;
}

_main_() {
    number x = 10;
    number y = 20;
    number sum = add(x, y);
    string hello = ""Hello, "";
    string world = ""world!"";
    string message = concatenate(hello, world);

    print(message);

    bool check = is_even(sum);
    if (check) {
        print(""Sum is even."");
    } else {
        print(""Sum is odd."");
    }
    number i = 0;
    while ( i < 5 ) {
        sum += i;
        i++;
    }

    string userInput = input();
    print(""You entered: "" + userInput);    
}";
            _compiler.Check(source);

            var expectedInput = "Tom";
            var expectedOutput = "Hello, world!\r\nSum is even.\r\nYou entered: Tom\r\n";
            using (var consoleInput = new ConsoleInput(expectedInput))
            {
                using (var consoleOutput = new ConsoleOutput())
                {
                    _compiler.Exec(source);
                    Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
                }
            }
        }

        [TestMethod]
        public void InfiniteWhileLoopPassesValidation()
        {
            string source = @"
func is_even() -> void {
    print(""Sum is even."");
}

_main_() {
    while(true){
        is_even();
    }
}";
            _compiler.Check(source);

        }

        [TestMethod]
        public void WhileLoopRunsExpectedNumberOfTimes()
        {
            string source = @"
_main_() {
    number i = 0;
    while ( i < 5 ) {
         i++;
         print(""While Test"");    
    }  
}";
            _compiler.Check(source);

            var expectedOutput = "While Test\r\nWhile Test\r\nWhile Test\r\nWhile Test\r\nWhile Test\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void IfElseWithFunctionConditionSelectsEvenBranch()
        {
            string source = @"
func is_even(number a) -> bool {
    return a % 2 == 0;
}

_main_() {
    number num = 12;           
    if (is_even(num)) {
        print(""Sum is even."");
    } else {
        print(""Sum is odd."");
    }   
}";

            _compiler.Check(source);

            var expectedOutput = "Sum is even.\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void IfElseWithLogicalOperatorsSelectsExpectedBranch()
        {
            string source = @"
_main_() {
    number num = 12;           
    if (num == 12 || num > 15 && num == 10) {
        print(""YES"");
    } else {
        print(""NO"");
    }   
}";

            _compiler.Check(source);

            var expectedOutput = "YES\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void FileExistReadAndWriteFunctionsPassesValidation()
        {
            string source = @"
number myVar;
string fileContent;

_main_() {
    if (file_exist(""test.txt"")) {
        fileContent = file_read(""test.txt"");
        print(fileContent);
    } else {
        print(""File does not exist, creating new file with content."");
        file_write(""test.txt"", ""Hello, world!"");
    }
}";

            _compiler.Check(source);
        }

        [TestMethod]
        public void FileDeleteFunctionPassesValidation()
        {
            string source = @"
_main_() {
    string file_path = ""test.txt"";
    if (file_exist(file_path)) {
        file_delete(file_path);
        print(""file deleted sucessfully"");
    } else {
        print(""File does not exist."");
    }
}";

            _compiler.Check(source);
            _compiler.Exec(source);

        }

        [TestMethod]
        public void ArrayDeclarationsPassesValidation()
        {
            string source = @"
number[] arr = [10, 20, 30];  // Example array declaration with initializer

_main_() {
    number[] anotherArr;  // Example array declaration without initializer
    anotherArr = [5, 10, 15];
    print(anotherArr[1]);
}";

            _compiler.Check(source);


            var expectedOutput = "10\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void ArrayIterationWithLengthExecutesCorrectly()
        {

            string source = @"
_main_() {
    // Declare an array of numbers
    number[] arr = [10, 20, 30, 40, 50];
    number i = 0;
    arr[2] = 5;
    while (i < array_len(arr)) {
        print(""Element at index "" + i + "": "" + arr[i]);
        i++;
    }                     
}";

            _compiler.Check(source);

            var expectedOutput = "Element at index 0: 10\r\nElement at index 1: 20\r\nElement at index 2: 5\r\nElement at index 3: 40\r\nElement at index 4: 50\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }

        }

        [TestMethod]
        public void ArrayAddRemoveAndLengthFunctionsExecuteCorrectly()
        {
            string source = @"
_main_() {                                        
    number[] arr = [10, 20, 30, 40, 50];
    arr = array_remove(arr , 2);
    arr = array_add(arr, 60);
    arr = array_add(arr, 70);
    number i = 0;            
    while (i < array_len(arr)) {
        print(""Element at index "" + i + "": "" + arr[i]);
        i++;
    }                     
}";

            _compiler.Check(source);

            var expectedOutput = "Element at index 0: 10\r\nElement at index 1: 20\r\nElement at index 2: 40\r\nElement at index 3: 50\r\nElement at index 4: 60\r\nElement at index 5: 70\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }

        }

        [TestMethod]
        public void IterativeFibonacciExecutesCorrectly()
        {
            string source = @"
_main_() {
    print(""Enter The Number Of Terms:"");
    number n = to_number(input());
    number f = 0; 
    number f1=-1; 
    number f2=1;
    print(""The Fibonacci Series is:"");
    while(n>0)
    {
        f=f1+f2;
        f1=f2;
        f2=f;
        print(f);
        n--;
    }                
}";
            _compiler.Check(source);
            //var result = _compiler.Exec(source);


            var expectedInput = "5";
            var expectedOutput = "Enter The Number Of Terms:\r\nThe Fibonacci Series is:\r\n0\r\n1\r\n1\r\n2\r\n3\r\n";
            using (var consoleInput = new ConsoleInput(expectedInput))
            {
                using (var consoleOutput = new ConsoleOutput())
                {
                    _compiler.Exec(source);
                    Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
                }
            }
        }

        [TestMethod]
        public void RecursiveFibonacciExecutesCorrectly()
        {
            string source = @"
func fib(number x) -> number 
{
   number result; 
   if (x == 1 || x == 0) {
      result = x;
   } else {
      result = fib(x - 1) + fib(x - 2);
   }
   return result;                                      
}

_main_() {
   number x = 20; 
   number i = 0;
   while(i <= x) {
      print(i + "" => "" + fib(i));
      i++;
   }
}";
            _compiler.Check(source);

            var expectedOutput = "0 => 0\r\n1 => 1\r\n2 => 1\r\n3 => 2\r\n4 => 3\r\n5 => 5\r\n6 => 8\r\n7 => 13\r\n8 => 21\r\n9 => 34\r\n10 => 55\r\n11 => 89\r\n12 => 144\r\n13 => 233\r\n14 => 377\r\n15 => 610\r\n16 => 987\r\n17 => 1597\r\n18 => 2584\r\n19 => 4181\r\n20 => 6765\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }

        }

        [TestMethod]
        public void FactorialFunctionExecutesCorrectly()
        {
            string source = @"
func Factorial(number n) -> number
{
	number result = 1;
	if( n > 1)
	{
		result = n * Factorial(n-1);
	}
	return result;
}

_main_()
{
	number a = 5;
	print(Factorial(a));
}
";
            _compiler.Check(source);


            var expectedOutput = "120\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void CheckNumberFunctionReturnsExpectedClassification()
        {
            string source = @"
func CheckNumber(number n) -> string
{
string result = """";
if (n > 0)
{
result = ""Positive"";
}
else
{
result = ""Non-Positive"";
}
return result;
}

 _main_()
{
print(CheckNumber(5));
}
";
            _compiler.Check(source);


            var expectedOutput = "Positive\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void ForLoopExecutesCorrectly()
        {
            string source = @"
_main_() {
	number i;
	for( i =0; i < 5; i++){
		print(""run count"");
	}	
}
";
            _compiler.Check(source);


            var expectedOutput = "run count\r\nrun count\r\nrun count\r\nrun count\r\nrun count\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void FibonacciWithMultipleReturnsAndForLoopExecutesCorrectly()
        {
            string source = @"
func fib(number x) -> number 
{
	//return x;
   if (x == 1 || x == 0) {
      return x;
   } 
   return fib(x - 1) + fib(x - 2);                                     
}


func chap(number content) -> void
{
	print(content);
}


_main_() {
   for(number i =0; i <= 20; i++) {
      chap(fib(i));
	  //print(fib(i));
   }
}
";
            _compiler.Check(source);


            var expectedOutput =
                "0\r\n1\r\n1\r\n2\r\n3\r\n5\r\n8\r\n13\r\n21\r\n34\r\n55\r\n89\r\n144\r\n233\r\n377\r\n610\r\n987\r\n1597\r\n2584\r\n4181\r\n6765\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void VoidFunctionWithEmptyReturnExecutesCorrectly()
        {
            string source = @"
func test(number i) -> void
{
	print(""yess"");
	if(i == 5){
		print(""five"");
		return;
	}
	print(""Nop"");
	return;
}

_main_(){
	test(5);
}";
            _compiler.Check(source);


            var expectedOutput = "yess\r\nfive\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void ImplicitlyTypedLocalVariablesExecuteCorrectly()
        {
            string source = @"
func myFunction(number a, number b) -> number {
    return a + b;
}

_main_() {
	let z = myFunction(2, 7);
	print(z);
    let x = 10;
	let i = 0;
    while ( i < 5 ) {
        x++;
		i++;
    }
	z = 40;
    print(x);
	let string_var1 = ""hassan"";
	let string_var = ""this is a test"" + "" => "" + string_var1 + "" -> "" + x;
	print(string_var);

}";
            _compiler.Check(source);


            var expectedOutput = "9\r\n15\r\nthis is a test => hassan -> 15\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void ImplicitlyTypedPackageInstanceExecutesCorrectly()
        {
            string source = @"
pkg myPackage {
	number var = 10;
	
	init(number input1)
	{
		var = input1;
	}

	public func Second(number std) -> number{
		return std + var;
	}	
}

_main_() {
	let classInstance = new myPackage(5);
	print(classInstance.Second(2));
}";
            _compiler.Check(source);


            var expectedOutput = "7\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void ImplicitlyTypedForLoopVariableExecutesCorrectly()
        {
            string source = @"
_main_() {
	for(let i =0; i < 5; i++){
		print(i + "" this is a test"");
	}
}";
            _compiler.Check(source);


            var expectedOutput = "0 this is a test\r\n1 this is a test\r\n2 this is a test\r\n3 this is a test\r\n4 this is a test\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void ImplicitlyTypedPackageFieldExecutesCorrectly()
        {
            string source = @"
pkg My{
    public let var = 1;
}
_main_(){
    let m = new My();
    print(m.var);
}";
            _compiler.Check(source);


            var expectedOutput = "1\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void TopLevelVariableAndPrintCreateImplicitMain()
        {
            string source = @"
let text = ""hello world"";
print(text);
";
            _compiler.Check(source);


            var expectedOutput = "hello world\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void TopLevelFunctionLoopAndPrintCreateImplicitMain()
        {
            string source = @"
func Add(number i, number j) -> number{
	return i + j;
}

for(let i =0; i <5; i++){
	print(""this is a test"");
}
print(Add(4, 6));
";
            _compiler.Check(source);


            var expectedOutput = "this is a test\r\nthis is a test\r\nthis is a test\r\nthis is a test\r\nthis is a test\r\n10\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void TopLevelPackageUsageCreatesImplicitMain()
        {
            string source = @"
pkg My{
	let var = 22;
	public func AddPrint(number i) -> number {
		return i + var;
	}
}
let m = new My();
print(m.AddPrint(3));
";
            _compiler.Check(source);


            var expectedOutput = "25\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void ExplicitMainExcludesTopLevelExecutableStatements()
        {
            string source = @"
pkg My{
	let var = 22;
	public func AddChap(number i) -> number {
		return i + var;
	}
}

func Add(number i, number j) -> number{
	return i + j;
}

let text = ""hello world"";
print(text);
for(let i =0; i <5; i++){
	print(""this is a test"");
}
print(Add(4, 6));
let m = new My();
print(m.AddChap(3));


_main_(){
	print(""YES"");
}";
            _compiler.Check(source);


            var expectedOutput = "YES\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void BubbleSortExecutesCorrectly()
        {
            string source = @"
func BubbleSort(number[] sort_array) -> number[]
{
	let n = array_len(sort_array);
	for (number i = 0; i < n - 1; i++)
	{
		for (number j = 0; j < n - i - 1; j++)
		{
			if (sort_array[j] > sort_array[j + 1])
			{
				// Swap arr[j] and arr[j + 1]
				number temp = sort_array[j];
				sort_array[j] = sort_array[j + 1];
				sort_array[j + 1] = temp;
			}
		}
	}	
	return sort_array;
}

func ArrayPrint(number[] print_array) -> void {
	let i = 0;
    while (i < array_len(print_array)) {
        print(""Element at index "" + i + "": "" + print_array[i]);
        i++;
    }    
}

_main_()
{
	print(""befor Bubble sorting"");
	number[] first_array = [64, 34, 25, 12, 22, 11, 90];
	ArrayPrint(first_array);
	print(""after Bubble sorting"");
	number[] second_array = BubbleSort(first_array);
	ArrayPrint(second_array);
}";
            _compiler.Check(source);


            var expectedOutput = "befor Bubble sorting\r\nElement at index 0: 64\r\nElement at index 1: 34\r\nElement at index 2: 25\r\nElement at index 3: 12\r\nElement at index 4: 22\r\nElement at index 5: 11\r\nElement at index 6: 90\r\nafter Bubble sorting\r\nElement at index 0: 11\r\nElement at index 1: 12\r\nElement at index 2: 22\r\nElement at index 3: 25\r\nElement at index 4: 34\r\nElement at index 5: 64\r\nElement at index 6: 90\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void IterativeBinarySearchExecutesCorrectly()
        {
            string source = @"
 func BinarySearchIterative(number[] bsi_array, number key) -> number
    {	
        let min = 0;
        let max = array_len(bsi_array) - 1;
		
		number mid;

        while (min <= max)
        {
			mid = min + max;
			mid = round_number(mid/2);
            if (bsi_array[mid] == key)
            {
                return mid;
            }
            else if (bsi_array[mid] < key)
            {
                min = mid + 1;
            }
            else
            {
                max = mid - 1;
            }
        }
        return -1; // Element not found
    }

_main_()
{
	number[] sortedArray = [ 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 ];
	let target = 7;
	let result = BinarySearchIterative(sortedArray, target);

	if (result != -1)
	{
		print(""Element found at index "" + result);
	}
	else
	{
		print(""Element not found in the array"");
	}

}";
            _compiler.Check(source);


            var expectedOutput = "Element found at index 3\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void RecursiveBinarySearchExecutesCorrectly()
        {
            string source = @"
func BinarySearch(number[] arr, number target, number left, number right) -> number
{
	if (right >= left)
	{
		number distance = right - left;
		number mid = round_number(left + distance / 2);

		// Check if the target is present at the mid
		if (arr[mid] == target)
		{
			return mid;
		}

		// If the target is smaller than mid, it must be in the left subarray
		if (arr[mid] > target)
		{
			return BinarySearch(arr, target, left, mid - 1);
		}

		// Otherwise, the target must be in the right subarray
		return BinarySearch(arr, target, mid + 1, right);
	}

	// Target is not present in the array
	return -1;
}


_main_()
{
	number[] array = [ 2, 3, 4, 10, 40 ];
	number globalTarget = 10;

	number result = BinarySearch(array, globalTarget, 0, array_len(array) - 1);

	if (result != -1)
	{
		print(""Element found at index => "" + result);
	}
	else
	{
		print(""Element not found in the array"");
	}
}
";
            _compiler.Check(source);


            var expectedOutput = "Element found at index => 3\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void QuickSortExecutesCorrectly()
        {
            string source = @"
func Swap(number[] array, number a, number b) -> void
{
	number temp = array[a];
	array[a] = array[b];
	array[b] = temp;
}
func Partition(number[] array, number low, number high) -> number
{
	number pivot = array[high];
	number i = low - 1;

	for (number j = low; j < high; j++)
	{
		if (array[j] < pivot)
		{
			i++;
			Swap(array, i, j);
		}
	}

	Swap(array, i + 1, high);
	return i + 1;
}
func QuickSort(number[] array, number low, number high) -> void
{
	if (low < high)
	{
		number pivotIndex = Partition(array, low, high);
		QuickSort(array, low, pivotIndex - 1);
		QuickSort(array, pivotIndex + 1, high);
	}
}
func ArrayPrint(number[] print_array) -> void {
	let i = 0;
    while (i < array_len(print_array)) {
        print(""Element at index "" + i + "": "" + print_array[i]);
        i++;
    }    
}
_main_()
{
	number[] array = [ 34, 7, 23, 32, 5, 62 ];
	QuickSort(array, 0, array_len(array) - 1);
	print(""Sorted array: "");
	ArrayPrint(array);
}";
            _compiler.Check(source);


            var expectedOutput = "Sorted array: \r\nElement at index 0: 5\r\nElement at index 1: 7\r\nElement at index 2: 23\r\nElement at index 3: 32\r\nElement at index 4: 34\r\nElement at index 5: 62\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void ContinueStatementSkipsExpectedIterations()
        {
            string source = @"
for(let i= 0; i <= 10; i++)
{	
	if( i%2 == 0)
	{
		continue;
	}
	print(i);
}";
            _compiler.Check(source);


            var expectedOutput = "1\r\n3\r\n5\r\n7\r\n9\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void BreakStatementStopsLoopAtExpectedIteration()
        {
            string source = @"
for(let i= 0; i <= 10; i++)
{	
	if( i == 5)
	{
		break;
	}
	print(i);
}";
            _compiler.Check(source);


            var expectedOutput = "0\r\n1\r\n2\r\n3\r\n4\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void PowerOfTwoDetectionExecutesCorrectly()
        {
            string source = @"
func PowerOfTwo(number n) -> bool  {
	  if(n == 0) return false;
      if(n == 1) return true;
      while(n!=1)
      {
          if(n % 2 != 0) return false;
          n /= 2;
      }
      return true;
}

for(let i = 0; i <= 100 ; i++)
	if(PowerOfTwo(i))
		print(i +"" is PowerOfTwo"");
	//else
	//	print(i +"" is not PowerOfTwo"");";
            _compiler.Check(source);


            var expectedOutput = "1 is PowerOfTwo\r\n2 is PowerOfTwo\r\n4 is PowerOfTwo\r\n8 is PowerOfTwo\r\n16 is PowerOfTwo\r\n32 is PowerOfTwo\r\n64 is PowerOfTwo\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void FizzBuzzExecutesCorrectly()
        {
            string source = @"
for (let i = 1; i <= 30; i++)
	// Check if the number is a multiple of both 3 and 5
	if (i % 3 == 0 && i % 5 == 0)	
		print(""FizzBuzz"");	
	// Check if the number is a multiple of 3
	else if (i % 3 == 0)	
		print(""Fizz"");	
	// Check if the number is a multiple of 5
	else if (i % 5 == 0)	
		print(""Buzz"");	
	// If the number is not a multiple of 3 or 5, print the number
	else
		print(i);";
            _compiler.Check(source);


            var expectedOutput = "1\r\n2\r\nFizz\r\n4\r\nBuzz\r\nFizz\r\n7\r\n8\r\nFizz\r\nBuzz\r\n11\r\nFizz\r\n13\r\n14\r\nFizzBuzz\r\n16\r\n17\r\nFizz\r\n19\r\nBuzz\r\nFizz\r\n22\r\n23\r\nFizz\r\nBuzz\r\n26\r\nFizz\r\n28\r\n29\r\nFizzBuzz\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void PrimeNumberGenerationExecutesCorrectly()
        {
            string source = @"
// Function to check if a number is prime
func isPrime(number num) -> bool {
    if (num <= 1) {
        return false;
    }
    for (let i = 2; i <= sqrt_number(num); i++) {
        if (num % i == 0) {
            return false;
        }
    }
    return true;
}

// Function to generate all prime numbers up to a given limit
func generatePrimesUpTo(number limit) -> number[]  {
    number[] primes = [];
    for (let i = 2; i <= limit; i++) {
        if (isPrime(i)) {
            primes = array_add(primes, i);
        }
    }
    return primes;
}

_main_(){
    let limit = 100; // You can change this limit to any number
    number[] local_primes = generatePrimesUpTo(limit);

    print(""Prime numbers up to "" + limit);
    for (let i = 0; i < array_len(local_primes); i++ ) {
        print(local_primes[i]);
    }
}";
            _compiler.Check(source);


            var expectedOutput = "Prime numbers up to 100\r\n2\r\n3\r\n5\r\n7\r\n11\r\n13\r\n17\r\n19\r\n23\r\n29\r\n31\r\n37\r\n41\r\n43\r\n47\r\n53\r\n59\r\n61\r\n67\r\n71\r\n73\r\n79\r\n83\r\n89\r\n97\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void InsertionSortExecutesCorrectly()
        {
            string source = @"
func InsertionSort(number[] arr) -> number[]
{
	number n = array_len(arr);
	for (let i = 1; i < n; i++)
	{
		let key = arr[i];
		let j = i - 1;

		// Move elements of arr[0..i-1], that are greater than key, to one position ahead of their current position
		while (j >= 0 && arr[j] > key)
		{
			arr[j + 1] = arr[j];
			j = j - 1;
		}
		arr[j + 1] = key;
	}	
	return arr;
}

func PrintArray(number[] printArray) -> void
{
	for (let i = 0; i < array_len(printArray); i++)
	{
		print(printArray[i]);
	}
}

number[] array = [ 12, 11, 13, 5, 6 ];
print(""Original array:"");
PrintArray(array);
array = InsertionSort(array);
print(""Sorted array:"");
PrintArray(array);";
            _compiler.Check(source);


            var expectedOutput = "Original array:\r\n12\r\n11\r\n13\r\n5\r\n6\r\nSorted array:\r\n5\r\n6\r\n11\r\n12\r\n13\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void SelectionSortExecutesCorrectly()
        {
            string source = @"
func SelectionSort(number[] array) -> number[]
{
	number n = array_len(array);
	for (number i = 0; i < n - 1; i++)
	{
		number minIndex = i;
		for (number j = i + 1; j < n; j++)
		{
			if (array[j] < array[minIndex])
			{
				minIndex = j;
			}
		}
		number temp = array[minIndex];
		array[minIndex] = array[i];
		array[i] = temp;
	}	
	return array;
}

func PrintArray(number[] printArray) -> void
{
	for (let i = 0; i < array_len(printArray); i++)
	{
		print(printArray[i]);
	}
}

number[] localArray = [ 64, 25, 12, 22, 11 ];
print(""Original array:"");
PrintArray(localArray);
localArray = SelectionSort(localArray);
print(""Sorted array:"");
PrintArray(localArray);";

            _compiler.Check(source);


            var expectedOutput = "Original array:\r\n64\r\n25\r\n12\r\n22\r\n11\r\nSorted array:\r\n11\r\n12\r\n22\r\n25\r\n64\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void CountingSortExecutesCorrectly()
        {
            string source = @"
func CountingSort(number[] array) -> number[]
{
	number max = GetMaxValue(array);
	let arrayLen = array_len(array);	
	
	number[] count = number[max +1];	
	number[] output = number[arrayLen];

	// Count the occurrences of each element
	for (number i = 0; i < arrayLen; i++)
	{
		count[array[i]] = count[array[i]] + 1;
	}

	// Update the count array to store the actual positions of elements
	for (number j = 1; j <= max; j++)
	{
		count[j] = count[j] + count[j - 1];
	}

	// Build the output array
	for (number k = arrayLen - 1; k >= 0; k--)
	{
		output[count[array[k]] - 1] = array[k];
		count[array[k]] = count[array[k]] - 1;
	}

	// Copy the sorted elements back to the original array
	for (number l = 0; l < arrayLen; l++)
	{
		array[l] = output[l];
	}
	return array;
}

func GetMaxValue(number[] array) -> number
{
	let max = array[0];
	for (let i = 1; i < array_len(array); i++)
	{
		if (array[i] > max)
		{
			max = array[i];
		}
	}
	return max;
}

func PrintArray(number[] printArray) -> void
{
	let n = array_len(printArray);
	for (let i = 0; i < n; i++)
	{
		print(printArray[i]);
	}
}

number[] localArray = [ 4, 2, 2, 8, 3, 3, 1, 7, 5, 6 ];
print(""Original array:"");
PrintArray(localArray);
localArray = CountingSort(localArray);
print(""Sorted array:"");
PrintArray(localArray);";
            _compiler.Check(source);


            var expectedOutput = "Original array:\r\n4\r\n2\r\n2\r\n8\r\n3\r\n3\r\n1\r\n7\r\n5\r\n6\r\nSorted array:\r\n1\r\n2\r\n2\r\n3\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void BingoSortExecutesCorrectly()
        {
            string source = @"
func BingoSort(number[] arr) -> number[] {
	number n = array_len(arr);
	number min = GetMinValue(arr);
	number max = GetMaxValue(arr);
	number nextBingo = max;
	number nextPos = 0;

	while (min < nextBingo)
	{
		number startPos = nextPos;
		for (number i = startPos; i < n; i++)
		{
			if (arr[i] == min)
			{
				// Swap elements
				number temp = arr[nextPos];
				arr[nextPos] = arr[i];
				arr[i] = temp;
				nextPos++;
			}
			else if (arr[i] < nextBingo)
			{
				nextBingo = arr[i];
			}
		}
		min = nextBingo;
		nextBingo = max;
	}	
	return arr;
}

func GetMaxValue(number[] array) -> number {
	let max = array[0];
	for (let i = 1; i < array_len(array); i++)
	{
		if (array[i] > max)
		{
			max = array[i];
		}
	}
	return max;
}

func GetMinValue(number[] array) -> number {
	let min = array[0];
	for (let i = 1; i < array_len(array); i++)
	{
		if (array[i] < min)
		{
			min = array[i];
		}
	}
	return min;
}

func PrintArray(number[] printArray) -> void {
	let n = array_len(printArray);
	for (let i = 0; i < n; i++)
	{
		print(printArray[i]);
	}
}

number[] localArray = [ 7, 15, 8, 5, 3, 11, 9, 4, 1, 6, 2];
localArray = BingoSort(localArray);
print(""Sorted array:"");
PrintArray(localArray);";
            _compiler.Check(source);

            var expectedOutput = "Sorted array:\r\n1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n9\r\n11\r\n15\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void FloatingPointAdditionExecutesCorrectly()
        {
            string source = @"
let num  = 5.2 + 8;
print(num);
";
            _compiler.Check(source);


            var expectedOutput = "13.2\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }
    }
}
