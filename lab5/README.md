Был переработан код прошлых лабораторных работ для ассинхрого поведения программы. Изменения затронули такие файлы как:
1) HelpLibrary\DataClasses\DataBaseWorker.cs
2) HelpLibrary\DataClasses\FileTrancser.cs
3) HelpLibrary\DataClasses\XmlGenerator.cs
4) lab3\FileManager.cs
5) lab3\Logger.cs 
6) lab3\Program.cs
7) lab4\DataManager.cs
8) lab4\Program.cs
Методы, переработанные под ассинхронную работу, соответсвуют TAP(https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)

Примечание: Logger.cs больше всего переработан, на случай использования другой бд, в которой регулярно будут поступать новые данные, которые необохимо будет обрабатывать и отправлять.



