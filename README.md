# CSharp-3-semester
lab 2
Мониторит папку SourceDirectory и проверeт, добавился ли файл или нет.
Если файл был добавлен - создает архив c зашифрованным файлом и переносит его в TargetDirectory.
Использует шифрование RSA.
Распаковывает и дешифрует файл.

lab 3 
Все конфигурационные свойства в файлах appsettings.json и config.xml. 
Для валидации файла config.xml - config.xsd. 
При запуске приложения конф. свойства считываются с соотвествующих файлов,
с помощью ConfigManager(менеджера конфигураций - получает на вход путь к файлу с помощью AppDomain(8 пункт). 
Для каждого конфигурационного блока - соответсвующая модель. 
В случаи ошибок создается файл c отчётом по ошибкам
