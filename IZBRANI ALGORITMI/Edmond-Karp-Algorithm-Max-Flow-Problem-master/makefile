CC = g++
all: MaxFlow
MaxFlow: MaxFlowTest.o MaxFlow.o
	g++ MaxFlowTest.o MaxFlow.o -o MaxFlow
MaxFlowTest.o: MaxFlowTest.cpp MaxFlow.h
	g++ -c MaxFlowTest.cpp -std=c++11
MaxFlow.o: MaxFlow.cpp MaxFlow.h
	g++ -c MaxFlow.cpp -std=c++11
clean:
	rm-rf *o MaxFlow