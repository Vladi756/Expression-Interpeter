# Expression-Interpeter

Arithmetic expression interpeter coded in C#. 
It generates an abstract syntax tree, and it honors operator precedence, as can be seen in the diagram below: 


![image](https://user-images.githubusercontent.com/74903538/134972278-bc24a2d7-625d-4ed5-a2fd-0333902c71d6.png)

The correct answer is outputted. The interpreter correctly adds 2 and 3, and then multiplies it by 2. 

## Error Handling 

The interpreter has error handling and can throw exceptions in case there are invalid expressions in the input. 
Here is an example of the error handling: 

![image](https://user-images.githubusercontent.com/74903538/134972492-74deb80b-5d19-4a83-90ef-2520011a7feb.png)

The error messages are displayed in red and even offer the user some guidance in that they tell the user which token is the one that needs to be changed - and which one was in fact expected. In the above example, a closed parenthesis is missing, and the interpreter correctly identifies this and lets the user know. 

## Motivation 

This project started out as an idea for a compiler. For the longest time, I was interested in compilers and how they work, however, after some background reading, I quickly realized I am not a good enough programmer to accomplish this feat. Therefore, I decided to try something easier - like a program which correctly interprets and evaluates arithmetic expressions - an Arithmetic Expression Interpreter. This is the first stepping stone in achieving my goal of coding a compiler. 
