using Benita;
namespace BenitaTestProject
{
    [TestClass]
    public class PackageTest
    {
        private CompilerClass _compiler;

        [TestInitialize]
        public void Setup()
        {
            _compiler = new CompilerClass();
        }


        [TestMethod]
        public void Test1() // check constructor
        {
            string source = @"
pkg myPackage {
	number var = 10;	
	func myPackage(number input1) -> void
	{
		var = input1;
	}
}

_main_(){
	number mn = 5;
	myPackage classInstance = new myPackage(5);
	print(classInstance.var);	
}";

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

}

int main()
{
  double mn = 5;
  myPackage classInstance = new myPackage(5);
  std::cout << classInstance.var << std::endl;
  return 0;
}".Trim();
            // Act            
            var generatedCode = _compiler.GenerateCppCode(source);
            //var result = _compiler.Exec(source);

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
        public void Test2() // check constructor
        {
            string source = @"
pkg myPackage {
	number var = 10;	
	func myPackage(number input1) -> void{
		var = input1;
	}
	func Third() -> number{
		return 3;
	}	
}
_main_(){
	number mn = 5;
	myPackage classInstance = new myPackage(5);

	print(classInstance.Third());
	classInstance.var += classInstance.Third();
	print(classInstance.var);
}";

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

  double Third()
  {
    return 3;
  }

}

int main()
{
  double mn = 5;
  myPackage classInstance = new myPackage(5);
  std::cout << classInstance.Third() << std::endl;
  classInstance.var += classInstance.Third();
  std::cout << classInstance.var << std::endl;
  return 0;
}".Trim();
            // Act            
            var generatedCode = _compiler.GenerateCppCode(source);
            //var result = _compiler.Exec(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "3\r\n8\r\n";

            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }

        }


        [TestMethod]
        public void Test() // check constructor
        {
            string source = @"
pkg myPackage {
	number var = 10;	
	func myPackage(number input1) -> void
	{
		var = input1;
	}
	func First(number c) -> void{
		print(c);
	}	
	func Second(number std) -> number{
		return std + var;
	}
	func Third() -> number{
		return 3;
	}	
}

func First(number c) -> void{
	print(c + 1);
}

func inc(number a) -> number{
return a + 3;
}


_main_(){
	number mn = 5;
	myPackage classInstance = new myPackage(5);

	print(classInstance.Third());
	classInstance.var += classInstance.Third();
	print(classInstance.var);
	
	classInstance.var = classInstance.Third() + 3;
	print(classInstance.var);

	classInstance.var +=3;
	print(classInstance.var);
	classInstance.var = mn + 3;
	

	classInstance.var = classInstance.var / 2;
	print(classInstance.var);
	print(inc(classInstance.var));
	classInstance.var = inc(classInstance.var);
	print(classInstance.var);
	classInstance.First(5);
	print(classInstance.Second(2));
	First(8);
	print(""this is a test"");	
}";

            string expectedCode = @"#include <iostream>
#include <string>
#include <vector>
#include <fstream>


class myPackage{
  double var = 10;
  void myPackage(double input1)
  {
    var = input1;
  }

  void First(double c)
  {
    std::cout << c << std::endl;
  }

  double Second(double std)
  {
    return std + var;
  }

  double Third()
  {
    return 3;
  }

}

void First(double c)
{
  std::cout << c << 1 << std::endl;
}

double inc(double a)
{
  return a + 3;
}

int main()
{
  double mn = 5;
  myPackage classInstance = new myPackage(5);
  std::cout << classInstance.Third() << std::endl;
  classInstance.var += classInstance.Third();
  std::cout << classInstance.var << std::endl;
  classInstance.var = classInstance.Third() + 3;
  std::cout << classInstance.var << std::endl;
  classInstance.var += 3;
  std::cout << classInstance.var << std::endl;
  classInstance.var = mn + 3;
  classInstance.var = classInstance.var / 2;
  std::cout << classInstance.var << std::endl;
  std::cout << inc(classInstance.var) << std::endl;
  classInstance.var = inc(classInstance.var);
  std::cout << classInstance.var << std::endl;
  classInstance.First(5);
  std::cout << classInstance.Second(2) << std::endl;
  First(8);
  std::cout << ""this is a test"" << std::endl;
  return 0;
}".Trim();
            // Act            
            var generatedCode = _compiler.GenerateCppCode(source);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());

            // Act
            var expectedOutput = "3\r\n8\r\n6\r\n9\r\n4\r\n7\r\n7\r\n5\r\n9\r\n9\r\nthis is a test\r\n";

            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                // Assert exc
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }

        }
    }
}
