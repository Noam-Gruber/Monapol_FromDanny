   M S B A M L   `   `   `  ����  MonopolyClientUI!    MonopolyClientUI.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Monopoly Client��5   	   $
��1200��6!   $
��1200��6/   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5	      $
��Auto �6   �� 5
      $��* �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��0��6.   $��0,0,0,10q�6;   .+�5      �� $��
Server IP:��6   $��Center=�6)   �� 5      -   5         ��Name$ IpAddressTextBox��5      $	��120��60   $	��5,0q�6<   �� 5      $��Port:��6   $��Center=�6$   �� 5      -   5        $ PortTextBox��5      $��60��6+   $	��5,0q�66   $
��5000��6C   �� 5      -   5         ��Name$ ConnectButton��5      $��Connect��6,   $
��10,0q�6Z   5      �� 5   
   $ 
Horizontal=�6   $��1��6.   $��0,0,0,10q�6;   .+�5      �� $��Player Name:��6   $��Center=�6+   �� 5      -   5        $ PlayerNameTextBox��5      $	��150��61   $	��5,0q�6=   �� 5      -   5        $ JoinGameButton��5      $��	Join Game��6-   $
��10,0q�6^   5       �� 5#   
   $��2��6   
���   �$��1q�62   $��0,0,0,10q�6F   $	��800��6X   $	��800��6e   ._�5%      � .+�5'      �� -   5         ��Name$ 
BoardImage��5'      $-��'pack://application:,,,/Images/board.jpg��6,   $��Uniform=�6]   �� 5*      -   5         ��Name$ OverlayCanvas��5*      
������ .+�5,      A� -   5         A�Name$ Player1Piece��5,      $��30��64   $��30��6?   
���  �� �� Left$ 50��5-      
	 �� Top$	 50��6   5.      50      51      �� 54   
   -	   5        $ MessagesTextBox��54      $��3��6+   $��0,0,0,10q�68   
 �� 
IsReadOnly$

 True��55      " �� VerticalScrollBarVisibility$
 Auto=�6$    �� AcceptsReturn$
 True��6G   $	��100��6\   �� 58   
   $ 
Horizontal=�6   $��4��6.   .+�59      �� -
   5        $ RollDiceButton��59      $��	Roll Dice��6-   ${�False��6^   $	��100��6p   5:      5;      5<      