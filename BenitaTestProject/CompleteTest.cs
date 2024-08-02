using Benita;

namespace BenitaTestProject
{
    [TestClass]
    public class CompleteTest
    {
        private CompilerClass _compiler;

        [TestInitialize]
        public void Setup()
        {
            _compiler = new CompilerClass();
        }

        [TestMethod]
        public void Test1()
        {
            string source = @"
_main_() {
    print(""This is a test"" + "" => ""  + 1);
}";

            // Act 
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


int main()
{
std::cout << ""This is a test"" + "" => "" << 1 << std::endl;
return 0;
}
".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert cgc
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "This is a test => 1\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test2()
        {
            string source = @"
number x = 10;
func add(number a, number b) -> number {
    number c = a + b;
    return a + b;
}

_main_() {
    number y = 20;
    number sum = add(x, y);
    print(sum);
    string message = ""Hello, World!"";
    print(message);
}";

            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


double x = 10;
double add(double a, double b)
{
double c = a + b;
return a + b;
}

int main()
{
double y = 20;
double sum = add(x, y);
std::cout << sum << std::endl;
std::string message = ""Hello, World!"";
std::cout << message << std::endl;
return 0;
}
".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            var expectedOutput = "30\r\nHello, World!\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }

        }

        [TestMethod]
        public void Test3()
        {
            string source = @"
// this is a comment
/* This is Multi Line
one line
Two line
Three line
Comment */                        
string global_var = ""This is global variable"";
_main_() {
    number x = 10;
    number y = 20;
    print(myFunction(x , y));
    print(global_var);
}
func myFunction(number a, number b) -> number {
    return a + b;
}";

            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


std::string global_var = ""This is global variable"";
double myFunction(double a, double b)
{
return a + b;
}

int main()
{
double x = 10;
double y = 20;
std::cout << myFunction(x, y) << std::endl;
std::cout << global_var << std::endl;
return 0;
}".Trim();
            // Act
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            var expectedOutput = "30\r\nThis is global variable\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }

        }

        [TestMethod]
        public void Test4()
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

            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


double myFunction(double a, double b)
{
return a + b;
}

int main()
{
double x = 10;
double y = 8;
std::cout << x << y << std::endl;
return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert cgc
            Assert.AreEqual(expectedCode, generatedCode.Trim());


            // Act
            var expectedOutput = "18\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test5()
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

            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


double myFunction(double a, double b)
{
return a + b;
}

int main()
{
double x = 10;
double y = 20;
std::cout << x << y << std::endl;
double i = 0;
while (i < 5)
{
x++;
i++;
}
std::cout << x << std::endl;
return 0;
}".Trim();
            // Act
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "30\r\n15\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }

        }

        [TestMethod]
        public void Test6()
        {
            string source = @"
func add(number a, number b) -> number {
    return a + b;
}

_main_() {
    number result = add(2, 3);
    print(result);
}";

            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


double add(double a, double b)
{
return a + b;
}

int main()
{
double result = add(2, 3);
std::cout << result << std::endl;
return 0;
}".Trim();
            // Act
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "5\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test7()
        {
            string source = @" 
_main_()  {
    number result = 2 + 3;
    print(result);
}";

            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


int main()
{
double result = 2 + 3;
std::cout << result << std::endl;
return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "5\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test8()
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

            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


double add(double a, double b)
{
return a + b;
}

std::string concatenate(std::string a, std::string b)
{
return a + b;
}

int main()
{
double x = 10;
double y = 20;
double sum = add(x, y);
std::string message = concatenate(""hello"", "" world"");
std::cout << message << std::endl;
return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "hello world\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test9() // input
        {
            string source = @"
_main_() {
    print(""Enter your name "");
    string message = input();
    print(""your name is "");
    print(message);
}";

            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>

std::string input();

int main()
{
std::cout << ""Enter your name "" << std::endl;
std::string message = input();
std::cout << ""your name is "" << std::endl;
std::cout << message << std::endl;
return 0;
}

std::string input()
{
    std::string inputString;
    std::getline(std::cin, inputString);
    return inputString;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedInput = "Tom";
            var expectedOutput = "Enter your name \r\nyour name is \r\nTom\r\n";
            using (var consoleInput = new ConsoleInput(expectedInput))
            {
                using (var consoleOutput = new ConsoleOutput())
                {
                    _compiler.Exec(source);
                    // Assert exc
                    Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
                }
            }
        }

        [TestMethod]
        public void Test10() // input
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

            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>

std::string input();

double add(double a, double b)
{
return a + b;
}

std::string concatenate(std::string a, std::string b)
{
return a + b;
}

bool is_even(double a)
{
return a % 2 == 0;
}

int main()
{
double x = 10;
double y = 20;
double sum = add(x, y);
std::string hello = ""Hello, "";
std::string world = ""world!"";
std::string message = concatenate(hello, world);
std::cout << message << std::endl;
bool check = is_even(sum);
if (check)
{
std::cout << ""Sum is even."" << std::endl;
}
else
{
std::cout << ""Sum is odd."" << std::endl;
}
double i = 0;
while (i < 5)
{
sum += i;
i++;
}
std::string userInput = input();
std::cout << ""You entered: "" << userInput << std::endl;
return 0;
}

std::string input()
{
    std::string inputString;
    std::getline(std::cin, inputString);
    return inputString;
}".Trim();
            // Act            
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());


            // Act
            var expectedInput = "Tom";
            var expectedOutput = "Hello, world!\r\nSum is even.\r\nYou entered: Tom\r\n";
            using (var consoleInput = new ConsoleInput(expectedInput))
            {
                using (var consoleOutput = new ConsoleOutput())
                {
                    _compiler.Exec(source);
                    // Assert exc
                    Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
                }
            }
        }

        [TestMethod]
        public void Test11() // Infinite loop
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

            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


void is_even()
{
std::cout << ""Sum is even."" << std::endl;
}

int main()
{
while (true)
{
is_even();
}
return 0;
}


".Trim();
            // Act            
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());
        }

        [TestMethod]
        public void Test12() //while
        {
            string source = @"
_main_() {
    number i = 0;
    while ( i < 5 ) {
         i++;
         print(""While Test"");    
    }  
}";

            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


int main()
{
double i = 0;
while (i < 5)
{
i++;
std::cout << ""While Test"" << std::endl;
}
return 0;
}".Trim();
            // Act            
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "While Test\r\nWhile Test\r\nWhile Test\r\nWhile Test\r\nWhile Test\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test13() // if else
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

            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


bool is_even(double a)
{
return a % 2 == 0;
}

int main()
{
double num = 12;
if (is_even(num))
{
std::cout << ""Sum is even."" << std::endl;
}
else
{
std::cout << ""Sum is odd."" << std::endl;
}
return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "Sum is even.\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test14() // if else or
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

            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


int main()
{
double num = 12;
if (num == 12 || num > 15 && num == 10)
{
std::cout << ""YES"" << std::endl;
}
else
{
std::cout << ""NO"" << std::endl;
}
return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "YES\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test15() // File => cgc only
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

            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>

bool file_exist(const std::string& filename);
std::string file_read(const std::string& filename);
void file_write(const std::string& filename, const std::string& content);

double myVar;
std::string fileContent;
int main()
{
if (file_exist(""test.txt""))
{
fileContent = file_read(""test.txt"");
std::cout << fileContent << std::endl;
}
else
{
std::cout << ""File does not exist, creating new file with content."" << std::endl;
file_write(""test.txt"", ""Hello, world!"");
}
return 0;
}

bool file_exist(const std::string& filename)
{
    std::ifstream file(filename);
    return file.is_open();
}

std::string file_read(const std::string& filename)
{
    std::ifstream file(filename);
    if (!file.is_open())
        throw std::runtime_error(""Could not open file"");
    std::string content((std::istreambuf_iterator<char>(file)), std::istreambuf_iterator<char>());
    file.close();
    return content;
}

void file_write(const std::string& filename, const std::string& content)
{
    std::ofstream file(filename);
    if (!file.is_open())
        throw std::runtime_error(""Could not open file"");
    file << content;
    file.close();
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            //var expectedOutput = "File does not exist, creating new file with content.\r\n";
            //using (var consoleOutput = new ConsoleOutput())
            //{
            //    _compiler.Exec(source);
            //    // Assert exc
            //    Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            //}
        }

        [TestMethod]
        public void Test16() // File Delete => cgc only
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

            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>
#include <filesystem>

bool file_exist(const std::string& filename);
bool file_delete(const std::string& filename);

int main()
{
std::string file_path = ""test.txt"";
if (file_exist(file_path))
{
file_delete(file_path);
std::cout << ""file deleted sucessfully"" << std::endl;
}
else
{
std::cout << ""File does not exist."" << std::endl;
}
return 0;
}

bool file_exist(const std::string& filename)
{
    std::ifstream file(filename);
    return file.is_open();
}

bool file_delete(const std::string& filename)
{
    if (std::filesystem::remove(filename))
        return true;
    return false;
}".Trim();
            // Act

            var generatedCode = _compiler.GenerateCppCode(source);
            _compiler.Exec(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());
        }

        [TestMethod]
        public void Test17() // Simple Array
        {
            string source = @"
number[] arr = [10, 20, 30];  // Example array declaration with initializer

_main_() {
    number[] anotherArr;  // Example array declaration without initializer
    anotherArr = [5, 10, 15];
    print(anotherArr[1]);
}";

            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


std::vector<double> arr = {10, 20, 30};
int main()
{
std::vector<double> anotherArr;
anotherArr = {5, 10, 15};
std::cout << anotherArr[1] << std::endl;
return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "10\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test18() // Array
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

            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>

template <typename T>
int array_len(const std::vector<T>& vec);

int main()
{
std::vector<double> arr = {10, 20, 30, 40, 50};
double i = 0;
arr[2] = 5;
while (i < array_len(arr))
{
std::cout << ""Element at index "" + i + "": "" << arr[i] << std::endl;
i++;
}
return 0;
}

int array_len(const std::vector<T>& vec)
{
    return static_cast<int>(vec.size());
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "Element at index 0: 10\r\nElement at index 1: 20\r\nElement at index 2: 5\r\nElement at index 3: 40\r\nElement at index 4: 50\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test19() // Array Func
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

            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>

template <typename T>
std::vector<T> array_remove(const std::vector<T>& vec, int index);
std::vector<T> array_add(const std::vector<T>& vec, const T& element);
int array_len(const std::vector<T>& vec);

int main()
{
std::vector<double> arr = {10, 20, 30, 40, 50};
arr = array_remove(arr, 2);
arr = array_add(arr, 60);
arr = array_add(arr, 70);
double i = 0;
while (i < array_len(arr))
{
std::cout << ""Element at index "" + i + "": "" << arr[i] << std::endl;
i++;
}
return 0;
}

std::vector<T> array_remove(const std::vector<T>& vec, int index)
{
    if (index >= vec.size()) {
        throw std::out_of_range(""Index is out of range."");
    }
    std::vector<T> newVec = vec;
    newVec.erase(newVec.begin() + index);
    return newVec;
}

std::vector<T> array_add(const std::vector<T>& vec, const T& element)
{
    std::vector<T> newVec = vec;
    newVec.push_back(element);
    return newVec;
}

int array_len(const std::vector<T>& vec)
{
    return static_cast<int>(vec.size());
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "Element at index 0: 10\r\nElement at index 1: 20\r\nElement at index 2: 40\r\nElement at index 3: 50\r\nElement at index 4: 60\r\nElement at index 5: 70\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }

        }

        [TestMethod]
        public void Test20() // Fibo
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

            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>

int to_number(const std::string& str);
std::string input();

int main()
{
std::cout << ""Enter The Number Of Terms:"" << std::endl;
double n = to_number(input());
double f = 0;
double f1 = -1;
double f2 = 1;
std::cout << ""The Fibonacci Series is:"" << std::endl;
while (n > 0)
{
f = f1 + f2;
f1 = f2;
f2 = f;
std::cout << f << std::endl;
n--;
}
return 0;
}

int to_number(const std::string& str)
{
    try {
        return std::stoi(str);
    } catch (...) {
        return 0;
    }
}

std::string input()
{
    std::string inputString;
    std::getline(std::cin, inputString);
    return inputString;
}".Trim();
            // Act            
            var generatedCode = _compiler.GenerateCppCode(source);
            //var result = _compiler.Exec(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedInput = "5";
            var expectedOutput = "Enter The Number Of Terms:\r\nThe Fibonacci Series is:\r\n0\r\n1\r\n1\r\n2\r\n3\r\n";
            using (var consoleInput = new ConsoleInput(expectedInput))
            {
                using (var consoleOutput = new ConsoleOutput())
                {
                    _compiler.Exec(source);
                    // Assert exc
                    Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
                }
            }
        }

        [TestMethod]
        public void Test21() // Fibo Recurtion
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


double fib(double x)
{
double result;
if (x == 1 || x == 0)
{
result = x;
}
else
{
result = fib(x - 1) + fib(x - 2);
}
return result;
}

int main()
{
double x = 20;
double i = 0;
while (i <= x)
{
std::cout << i + "" => "" << fib(i) << std::endl;
i++;
}
return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);
            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "0 => 0\r\n1 => 1\r\n2 => 1\r\n3 => 2\r\n4 => 3\r\n5 => 5\r\n6 => 8\r\n7 => 13\r\n8 => 21\r\n9 => 34\r\n10 => 55\r\n11 => 89\r\n12 => 144\r\n13 => 233\r\n14 => 377\r\n15 => 610\r\n16 => 987\r\n17 => 1597\r\n18 => 2584\r\n19 => 4181\r\n20 => 6765\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test22() // Factorial
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


double Factorial(double n)
{
double result = 1;
if (n > 1)
{
result = n * Factorial(n - 1);
}
return result;
}

int main()
{
double a = 5;
std::cout << Factorial(a) << std::endl;
return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "120\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test23() // CheckNumber
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


std::string CheckNumber(double n)
{
std::string result = """";
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

int main()
{
std::cout << CheckNumber(5) << std::endl;
return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "Positive\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test24() // Test for
        {
            string source = @"
_main_() {
	number i;
	for( i =0; i < 5; i++;){
		print(""run count"");
	}	
}
";
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


int main()
{
double i;
for (i = 0;i < 5;i++)
{
std::cout << ""run count"" << std::endl;
}
return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "run count\r\nrun count\r\nrun count\r\nrun count\r\nrun count\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test25() // Test fib with 2 return and for
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
   for(number i =0; i <= 20; i++;) {
      chap(fib(i));
	  //print(fib(i));
   }
}
";
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


double fib(double x)
{
if (x == 1 || x == 0)
{
return x;
}
return fib(x - 1) + fib(x - 2);
}

void chap(double content)
{
std::cout << content << std::endl;
}

int main()
{
for (double i = 0;i <= 20;i++)
{
chap(fib(i));
}
return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput =
                "0\r\n1\r\n1\r\n2\r\n3\r\n5\r\n8\r\n13\r\n21\r\n34\r\n55\r\n89\r\n144\r\n233\r\n377\r\n610\r\n987\r\n1597\r\n2584\r\n4181\r\n6765\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test26() // return without parameter
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


void test(double i)
{
std::cout << ""yess"" << std::endl;
if (i == 5)
{
std::cout << ""five"" << std::endl;
return;
}
std::cout << ""Nop"" << std::endl;
return;
}

int main()
{
test(5);
return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "yess\r\nfive\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

    }
}
