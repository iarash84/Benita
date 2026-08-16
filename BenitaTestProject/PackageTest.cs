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
        public void PackageConstructor_WithInitialValue_ExecutesCorrectly()
        {
            string source = @"
pkg myPackage {
	public number var = 10;
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
            // Act            
            _compiler.Check(source);
            //var result = _compiler.Exec(source);

            // Assert

            // Act
            var expectedOutput = "5\r\n";

            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }

        }

        [TestMethod]
        public void PackageMethods_WhenInvoked_ExecuteCorrectly()
        {
            string source = @"
pkg myPackage {
	public number var = 10;
	func myPackage(number input1) -> void{
		var = input1;
	}
	public func Third() -> number{
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
            // Act            
            _compiler.Check(source);
            //var result = _compiler.Exec(source);

            // Assert

            // Act
            var expectedOutput = "3\r\n8\r\n";

            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }

        }


        [TestMethod]
        public void PackageMembers_WithMixedOperations_ExecuteCorrectly()
        {
            string source = @"
pkg myPackage {
	public number var = 10;
	func myPackage(number input1) -> void
	{
		var = input1;
	}
	public func First(number c) -> void{
		print(c);
	}	
	public func Second(number std) -> number{
		return std + var;
	}
	public func Third() -> number{
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
            // Act            
            _compiler.Check(source);

            // Assert

            // Act
            var expectedOutput = "3\r\n8\r\n6\r\n9\r\n4\r\n7\r\n7\r\n5\r\n9\r\n9\r\nthis is a test\r\n";

            using (var consoleOutput = new ConsoleOutput())
            {
                _compiler.Exec(source);
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }

        }
    }
}
