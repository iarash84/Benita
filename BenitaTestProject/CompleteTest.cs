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
}".Trim();
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
}".Trim();
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
}

".Trim();
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
}".Trim();
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
}

".Trim();
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
	for( i =0; i < 5; i++){
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
   for(number i =0; i <= 20; i++) {
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

        [TestMethod]
        public void Test27() // let implicitly typed local variables
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
  double z = myFunction(2, 7);
  std::cout << z << std::endl;
  double x = 10;
  double i = 0;
  while (i < 5)
  {
    x++;
    i++;
  }
  z = 40;
  std::cout << x << std::endl;
  std::string string_var1 = ""hassan"";
  auto string_var = ""this is a test"" + "" => "" + string_var1 + "" -> "" + x;
  std::cout << string_var << std::endl;
  return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "9\r\n15\r\nthis is a test => hassan -> 15\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test28() // let implicitly typed local variables for instance class
        {
            string source = @"
pkg myPackage {
	number var = 10;
	
	func myPackage(number input1) -> void
	{
		var = input1;
	}

	func Second(number std) -> number{
		return std + var;
	}	
}

_main_() {
	let classInstance = new myPackage(5);
	print(classInstance.Second(2));
}";
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


class myPackage{
  double var = 10;
  void myPackage(double input1)
  {
    var = input1;
  }

  double Second(double std)
  {
    return std + var;
  }

}

int main()
{
  myPackage classInstance = new myPackage(5);
  std::cout << classInstance.Second(2) << std::endl;
  return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "7\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test29() // let implicitly typed local variables in for statment
        {
            string source = @"
_main_() {
	for(let i =0; i < 5; i++){
		print(i + "" this is a test"");
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
  for (double i = 0;i < 5;i++)
  {
    std::cout << i << "" this is a test"" << std::endl;
  }
  return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "0 this is a test\r\n1 this is a test\r\n2 this is a test\r\n3 this is a test\r\n4 this is a test\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test30() // let implicitly typed local variables in package
        {
            string source = @"
pkg My{
    let var = 1;
}
_main_(){
    let m = new My();
    print(m.var);
}";
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


class My{
  double var = 1;
}

int main()
{
  My m = new My();
  std::cout << m.var << std::endl;
  return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "1\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test31() // Code with no _main_
        {
            string source = @"
let text = ""hello world"";
print(text);
";
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


std::string text = ""hello world"";
int main()
{
  std::cout << text << std::endl;
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
        public void Test32() // Code with no _main_
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


double Add(double i, double j)
{
  return i + j;
}

int main()
{
  for (double i = 0;i < 5;i++)
  {
    std::cout << ""this is a test"" << std::endl;
  }
  std::cout << Add(4, 6) << std::endl;
  return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "this is a test\r\nthis is a test\r\nthis is a test\r\nthis is a test\r\nthis is a test\r\n10\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test33() // Code with no _main_
        {
            string source = @"
pkg My{
	let var = 22;
	func AddPrint(number i) -> number {
		return i + var;
	}
}
let m = new My();
print(m.AddPrint(3));
";
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


class My{
  double var = 22;
  double AddPrint(double i)
  {
    return i + var;
  }

}

int main()
{
  My m = new My();
  std::cout << m.AddPrint(3) << std::endl;
  return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "25\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test34() // Code with _main_
        {
            string source = @"
pkg My{
	let var = 22;
	func AddChap(number i) -> number {
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


class My{
  double var = 22;
  double AddChap(double i)
  {
    return i + var;
  }

}

std::string text = ""hello world"";
double Add(double i, double j)
{
  return i + j;
}

int main()
{
  std::cout << ""YES"" << std::endl;
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
        public void Test35() //Bubble Sort
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>

template <typename T>
int array_len(const std::vector<T>& vec);

std::vector<double> BubbleSort(std::vector<double> sort_array)
{
  auto n = array_len(sort_array);
  for (double i = 0;i < n - 1;i++)
  {
    for (double j = 0;j < n - i - 1;j++)
    {
      if (sort_array[j] > sort_array[j + 1])
      {
        double temp = sort_array[j];
        sort_array[j] = sort_array[j + 1];
        sort_array[j + 1] = temp;
      }
    }
  }
  return sort_array;
}

void ArrayPrint(std::vector<double> print_array)
{
  double i = 0;
  while (i < array_len(print_array))
  {
    std::cout << ""Element at index "" + i + "": "" << print_array[i] << std::endl;
    i++;
  }
}

int main()
{
  std::cout << ""befor Bubble sorting"" << std::endl;
  std::vector<double> first_array = {64, 34, 25, 12, 22, 11, 90};
  ArrayPrint(first_array);
  std::cout << ""after Bubble sorting"" << std::endl;
  std::vector<double> second_array = BubbleSort(first_array);
  ArrayPrint(second_array);
  return 0;
}

int array_len(const std::vector<T>& vec)
{
    return static_cast<int>(vec.size());
}

".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "befor Bubble sorting\r\nElement at index 0: 64\r\nElement at index 1: 34\r\nElement at index 2: 25\r\nElement at index 3: 12\r\nElement at index 4: 22\r\nElement at index 5: 11\r\nElement at index 6: 90\r\nafter Bubble sorting\r\nElement at index 0: 11\r\nElement at index 1: 12\r\nElement at index 2: 22\r\nElement at index 3: 25\r\nElement at index 4: 34\r\nElement at index 5: 64\r\nElement at index 6: 90\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test36() //Binary Search
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>
#include <cmath>

template <typename T>
int array_len(const std::vector<T>& vec);

double BinarySearchIterative(std::vector<double> bsi_array, double key)
{
  double min = 0;
  auto max = array_len(bsi_array) - 1;
  double mid;
  while (min <= max)
  {
    mid = min + max;
    mid = std::round(mid / 2);
    if (bsi_array[mid] == key)
    {
      return mid;
    }
    else
    {
      if (bsi_array[mid] < key)
      {
        min = mid + 1;
      }
      else
      {
        max = mid - 1;
      }
    }
  }
  return -1;
}

int main()
{
  std::vector<double> sortedArray = {1, 3, 5, 7, 9, 11, 13, 15, 17, 19};
  double target = 7;
  double result = BinarySearchIterative(sortedArray, target);
  if (result != -1)
  {
    std::cout << ""Element found at index "" << result << std::endl;
  }
  else
  {
    std::cout << ""Element not found in the array"" << std::endl;
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
            var expectedOutput = "Element found at index 3\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test37() //Binary search recursive
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>
#include <cmath>

template <typename T>
int array_len(const std::vector<T>& vec);

double BinarySearch(std::vector<double> arr, double target, double left, double right)
{
  if (right >= left)
  {
    double distance = right - left;
    double mid = std::round(left + distance / 2);
    if (arr[mid] == target)
    {
      return mid;
    }
    if (arr[mid] > target)
    {
      return BinarySearch(arr, target, left, mid - 1);
    }
    return BinarySearch(arr, target, mid + 1, right);
  }
  return -1;
}

int main()
{
  std::vector<double> array = {2, 3, 4, 10, 40};
  double globalTarget = 10;
  double result = BinarySearch(array, globalTarget, 0, array_len(array) - 1);
  if (result != -1)
  {
    std::cout << ""Element found at index => "" << result << std::endl;
  }
  else
  {
    std::cout << ""Element not found in the array"" << std::endl;
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
            var expectedOutput = "Element found at index => 3\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test38() //QuickSort
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>

template <typename T>
int array_len(const std::vector<T>& vec);

void Swap(std::vector<double> array, double a, double b)
{
  double temp = array[a];
  array[a] = array[b];
  array[b] = temp;
}

double Partition(std::vector<double> array, double low, double high)
{
  double pivot = array[high];
  double i = low - 1;
  for (double j = low;j < high;j++)
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

void QuickSort(std::vector<double> array, double low, double high)
{
  if (low < high)
  {
    double pivotIndex = Partition(array, low, high);
    QuickSort(array, low, pivotIndex - 1);
    QuickSort(array, pivotIndex + 1, high);
  }
}

void ArrayPrint(std::vector<double> print_array)
{
  double i = 0;
  while (i < array_len(print_array))
  {
    std::cout << ""Element at index "" + i + "": "" << print_array[i] << std::endl;
    i++;
  }
}

int main()
{
  std::vector<double> array = {34, 7, 23, 32, 5, 62};
  QuickSort(array, 0, array_len(array) - 1);
  std::cout << ""Sorted array: "" << std::endl;
  ArrayPrint(array);
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
            var expectedOutput = "Sorted array: \r\nElement at index 0: 5\r\nElement at index 1: 7\r\nElement at index 2: 23\r\nElement at index 3: 32\r\nElement at index 4: 34\r\nElement at index 5: 62\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test39() //continue
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


int main()
{
  for (double i = 0;i <= 10;i++)
  {
    if (i % 2 == 0)
    {
      continue;
    }
    std::cout << i << std::endl;
  }
  return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "1\r\n3\r\n5\r\n7\r\n9\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test40() //break
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


int main()
{
  for (double i = 0;i <= 10;i++)
  {
    if (i == 5)
    {
      break;
    }
    std::cout << i << std::endl;
  }
  return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "0\r\n1\r\n2\r\n3\r\n4\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test41() //PowerOfTwo
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


bool PowerOfTwo(double n)
{
  if (n == 0)
  {
    return false;
  }
  if (n == 1)
  {
    return true;
  }
  while (n != 1)
  {
    if (n % 2 != 0)
    {
      return false;
    }
    n /= 2;
  }
  return true;
}

int main()
{
  for (double i = 0;i <= 100;i++)
  {
    if (PowerOfTwo(i))
    {
      std::cout << i << "" is PowerOfTwo"" << std::endl;
    }
  }
  return 0;
}

".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "1 is PowerOfTwo\r\n2 is PowerOfTwo\r\n4 is PowerOfTwo\r\n8 is PowerOfTwo\r\n16 is PowerOfTwo\r\n32 is PowerOfTwo\r\n64 is PowerOfTwo\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test42() //FizzBuzz
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


int main()
{
  for (double i = 1;i <= 30;i++)
  {
    if (i % 3 == 0 && i % 5 == 0)
    {
      std::cout << ""FizzBuzz"" << std::endl;
    }
    else
    {
      if (i % 3 == 0)
      {
        std::cout << ""Fizz"" << std::endl;
      }
      else
      {
        if (i % 5 == 0)
        {
          std::cout << ""Buzz"" << std::endl;
        }
        else
        {
          std::cout << i << std::endl;
        }
      }
    }
  }
  return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "1\r\n2\r\nFizz\r\n4\r\nBuzz\r\nFizz\r\n7\r\n8\r\nFizz\r\nBuzz\r\n11\r\nFizz\r\n13\r\n14\r\nFizzBuzz\r\n16\r\n17\r\nFizz\r\n19\r\nBuzz\r\nFizz\r\n22\r\n23\r\nFizz\r\nBuzz\r\n26\r\nFizz\r\n28\r\n29\r\nFizzBuzz\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test43() //isPrime
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

    print(""Prime numbers up to "" + limit)
    for (let i = 0; i < array_len(local_primes); i++ ) {
        print(local_primes[i]);
    }
}";
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>
#include <cmath>

template <typename T>
std::vector<T> array_add(const std::vector<T>& vec, const T& element);
int array_len(const std::vector<T>& vec);

bool isPrime(double num)
{
  if (num <= 1)
  {
    return false;
  }
  for (double i = 2;i <= std::sqrt(num);i++)
  {
    if (num % i == 0)
    {
      return false;
    }
  }
  return true;
}

std::vector<double> generatePrimesUpTo(double limit)
{
  std::vector<double> primes = {};
  for (double i = 2;i <= limit;i++)
  {
    if (isPrime(i))
    {
      primes = array_add(primes, i);
    }
  }
  return primes;
}

int main()
{
  double limit = 100;
  std::vector<double> local_primes = generatePrimesUpTo(limit);
  std::cout << ""Prime numbers up to "" << limit << std::endl;
  for (double i = 0;i < array_len(local_primes);i++)
  {
    std::cout << local_primes[i] << std::endl;
  }
  return 0;
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
            var expectedOutput = "Prime numbers up to 100\r\n2\r\n3\r\n5\r\n7\r\n11\r\n13\r\n17\r\n19\r\n23\r\n29\r\n31\r\n37\r\n41\r\n43\r\n47\r\n53\r\n59\r\n61\r\n67\r\n71\r\n73\r\n79\r\n83\r\n89\r\n97\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test44() //Insertion Sort
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>

template <typename T>
int array_len(const std::vector<T>& vec);

std::vector<double> array = {12, 11, 13, 5, 6};
std::vector<double> InsertionSort(std::vector<double> arr)
{
  double n = array_len(arr);
  for (double i = 1;i < n;i++)
  {
    auto key = arr[i];
    auto j = i - 1;
    while (j >= 0 && arr[j] > key)
    {
      arr[j + 1] = arr[j];
      j = j - 1;
    }
    arr[j + 1] = key;
  }
  return arr;
}

void PrintArray(std::vector<double> printArray)
{
  for (double i = 0;i < array_len(printArray);i++)
  {
    std::cout << printArray[i] << std::endl;
  }
}

int main()
{
  std::cout << ""Original array:"" << std::endl;
  PrintArray(array);
  array = InsertionSort(array);
  std::cout << ""Sorted array:"" << std::endl;
  PrintArray(array);
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
            var expectedOutput = "Original array:\r\n12\r\n11\r\n13\r\n5\r\n6\r\nSorted array:\r\n5\r\n6\r\n11\r\n12\r\n13\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test45() //Selection Sort
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

            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>

template <typename T>
int array_len(const std::vector<T>& vec);

std::vector<double> localArray = {64, 25, 12, 22, 11};
std::vector<double> SelectionSort(std::vector<double> array)
{
  double n = array_len(array);
  for (double i = 0;i < n - 1;i++)
  {
    double minIndex = i;
    for (double j = i + 1;j < n;j++)
    {
      if (array[j] < array[minIndex])
      {
        minIndex = j;
      }
    }
    double temp = array[minIndex];
    array[minIndex] = array[i];
    array[i] = temp;
  }
  return array;
}

void PrintArray(std::vector<double> printArray)
{
  for (double i = 0;i < array_len(printArray);i++)
  {
    std::cout << printArray[i] << std::endl;
  }
}

int main()
{
  std::cout << ""Original array:"" << std::endl;
  PrintArray(localArray);
  localArray = SelectionSort(localArray);
  std::cout << ""Sorted array:"" << std::endl;
  PrintArray(localArray);
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
            var expectedOutput = "Original array:\r\n64\r\n25\r\n12\r\n22\r\n11\r\nSorted array:\r\n11\r\n12\r\n22\r\n25\r\n64\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test46() //Counting Sort
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>

template <typename T>
int array_len(const std::vector<T>& vec);

std::vector<double> localArray = {4, 2, 2, 8, 3, 3, 1, 7, 5, 6};
std::vector<double> CountingSort(std::vector<double> array)
{
  double max = GetMaxValue(array);
  auto arrayLen = array_len(array);
  std::vector<double> count(max + 1, 0);
  std::vector<double> output(arrayLen, 0);
  for (double i = 0;i < arrayLen;i++)
  {
    count[array[i]] = count[array[i]] + 1;
  }
  for (double j = 1;j <= max;j++)
  {
    count[j] = count[j] + count[j - 1];
  }
  for (double k = arrayLen - 1;k >= 0;k--)
  {
    output[count[array[k]] - 1] = array[k];
    count[array[k]] = count[array[k]] - 1;
  }
  for (double l = 0;l < arrayLen;l++)
  {
    array[l] = output[l];
  }
  return array;
}

double GetMaxValue(std::vector<double> array)
{
  auto max = array[0];
  for (double i = 1;i < array_len(array);i++)
  {
    if (array[i] > max)
    {
      max = array[i];
    }
  }
  return max;
}

void PrintArray(std::vector<double> printArray)
{
  auto n = array_len(printArray);
  for (double i = 0;i < n;i++)
  {
    std::cout << printArray[i] << std::endl;
  }
}

int main()
{
  std::cout << ""Original array:"" << std::endl;
  PrintArray(localArray);
  localArray = CountingSort(localArray);
  std::cout << ""Sorted array:"" << std::endl;
  PrintArray(localArray);
  return 0;
}

int array_len(const std::vector<T>& vec)
{
    return static_cast<int>(vec.size());
}

".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "Original array:\r\n4\r\n2\r\n2\r\n8\r\n3\r\n3\r\n1\r\n7\r\n5\r\n6\r\nSorted array:\r\n1\r\n2\r\n2\r\n3\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test47() //Bingo Sort
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
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>

template <typename T>
int array_len(const std::vector<T>& vec);

std::vector<double> localArray = {7, 15, 8, 5, 3, 11, 9, 4, 1, 6, 2};
std::vector<double> BingoSort(std::vector<double> arr)
{
  double n = array_len(arr);
  double min = GetMinValue(arr);
  double max = GetMaxValue(arr);
  double nextBingo = max;
  double nextPos = 0;
  while (min < nextBingo)
  {
    double startPos = nextPos;
    for (double i = startPos;i < n;i++)
    {
      if (arr[i] == min)
      {
        double temp = arr[nextPos];
        arr[nextPos] = arr[i];
        arr[i] = temp;
        nextPos++;
      }
      else
      {
        if (arr[i] < nextBingo)
        {
          nextBingo = arr[i];
        }
      }
    }
    min = nextBingo;
    nextBingo = max;
  }
  return arr;
}

double GetMaxValue(std::vector<double> array)
{
  auto max = array[0];
  for (double i = 1;i < array_len(array);i++)
  {
    if (array[i] > max)
    {
      max = array[i];
    }
  }
  return max;
}

double GetMinValue(std::vector<double> array)
{
  auto min = array[0];
  for (double i = 1;i < array_len(array);i++)
  {
    if (array[i] < min)
    {
      min = array[i];
    }
  }
  return min;
}

void PrintArray(std::vector<double> printArray)
{
  auto n = array_len(printArray);
  for (double i = 0;i < n;i++)
  {
    std::cout << printArray[i] << std::endl;
  }
}

int main()
{
  localArray = BingoSort(localArray);
  std::cout << ""Sorted array:"" << std::endl;
  PrintArray(localArray);
  return 0;
}

int array_len(const std::vector<T>& vec)
{
    return static_cast<int>(vec.size());
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            //Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "Sorted array:\r\n1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n9\r\n11\r\n15\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void Test48() //Float test
        {
            string source = @"
let num  = 5.2 + 8;
print(num);
";
            // Act
            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


auto num = 5.2 + 8;
int main()
{
  std::cout << num << std::endl;
  return 0;
}".Trim();
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "13.2\r\n";
            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }
    }
}
