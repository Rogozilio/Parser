   M S B A M L   `   `   `  ���� H  DParserDesktop, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:ParserDesktopParserDesktop      ParserDesktop.MainWindow   W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -localclr-namespace:ParserDesktop       � Title$  Parser��$	��450��$	��800��.���  � RowDefinitions �� $
��0.1* ��� $��0.08* ��� $
��0.5* ��� $
��0.4* �.+��� $��1�� �� Orientation$ 
Horizontal=�$��Center=�.+��� $	��100��$)��#Общее время работы:���� -   $��labelStopWatch��$
��Bold"�$��0 6 10 6q�$��00:00:00���� $	��100��$;��5Время до следующего запроса: ���� -   $��labelTimeRequest��$
��Bold"�$��0 6 10 6q�$��00:00:00���� $	��100��$%��Найдено квартир: ���� -   $��labelCountApartments��$
��Bold"�$��0 6 10 6q�$��0���� $	��100��$$��Найдено ошибок: :���� -   $��labelCountErrors��$
��Bold"�$��0 6 10 6q�$��0���� $��0��$��25��$��Center=�$ 
Horizontal=�.+��� $����Введите количество секунд между запросами (рекомендуемое значение 50) - ���� -   $��textTimeRequest�� �� SelectedText$ 50��$��30��$��25�� �� TextAlignment$ Center=�$��Center=��� -   $��buttonStart��$��Запуск��$�� 5 0 0 0q�$��2q��� -   $��checkBoxRU09��$
��RU09�� d� 	IsChecked$
 TrueY�$��Center=�$��5q��� -   $��checkBoxAvito��$��Avito��$
 TrueY�$��Center=�$��5q��� -	   $��checkBoxYla��$	��Yla��$
 TrueY�$��Center=�$��5q��� $��2��.���� -
   $��BlockApartments�� �� 
IsReadOnly$
 True��$��0q�$
��Bold"�$��7q�$(��"Квартир не найдено��$��10q�
��������� $��3��.���� -   $��BlockErrors��$
 True��$��0q�$
��Bold"�$��7q� �� TextWrapping$
 Wrap=�$,��&Ошибок не обнаружено��$��10q�
�������