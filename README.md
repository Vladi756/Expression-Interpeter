# Expression-Interpeter

Arithmetic expression interpeter coded in C#. 
It generates an abstract syntax tree, and it understands operator precedence. 

Additionally, the interpreter has error handling and can throw exceptions in case there are invalid expressions in the input. 

![image](https://user-images.githubusercontent.com/74903538/134972278-bc24a2d7-625d-4ed5-a2fd-0333902c71d6.png)

The above is an example of a typical input expression. The unix-style tree is outputted with a dark gray color and as you can see, the result is correct. 

Here is an example of the error handling: 

![image](https://user-images.githubusercontent.com/74903538/134972492-74deb80b-5d19-4a83-90ef-2520011a7feb.png)

The error messages are displayed in red and even offer the user some guidance in that they tell the user which token was exptected. 
