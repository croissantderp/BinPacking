# BinPacking
Genetic approach to the bin packing problem

Initially created to find the optimal way to fit various parts onto a couple 48" wood planks.

Uses a genetic algorithm with tounament selection, one-point crossover, and random mutations to determine the best solution

example input:  
objects to fit into containers: 22.2,21,2.55,1.62,7,9,9.5,10,9.1,17,19,0.55,15,22.2,21,2.55,1.62,7,9,9.5,10,9.1,17,19,0.55,15  
container size: 48  

example output:  
found:  
\[22.2, 1.62, 0.55, 21, 0.55]  
\[21, 9, 2.55, 10]  
\[2.55, 22.2, 9.5]  
\[7, 15, 9, 9.1]  
\[9.5, 9.1, 17, 7]  
\[10, 1.62, 17, 15]  
\[19, 19]  
7 containers and a score of 5
