#include <iostream>
#include <string>
#include <fstream>
#include <vector>
#include <algorithm>
using namespace std;

void countingSort(vector<int>stevila, vector<int>numbers, int minValue){


        for(int i=1; i<=stevila.size(); i++){ //sestevas trenutno polje z prejsnjim
            stevila[i]=stevila[i]+stevila[i-1];
        }

        vector<int> konec;
        for(int i=0; i<numbers.size(); i++){
            konec.push_back(0);
        }
        for(int i=numbers.size(); i>=0; i--){
            konec[stevila[numbers[i]]-1] = numbers[i];
            stevila[numbers[i]]=stevila[numbers[i]]-1;
        }

        ofstream izpis("out.txt"); // ustvari datoteko
        for(int i=0; i<konec.size(); i++){//nazaj pristejes kar si na zacetku odstel in IZPIS
        	izpis << konec[i]+minValue<<" ";
        }
}

void romanSort(vector<int>stevila, int minValue){

    vector<int> konec;

    for(int i=0; i<stevila.size(); i++){
       while(stevila[i]!=0){
            konec.push_back(i);
            stevila[i]--;
       }
    }

    ofstream izpis("out.txt"); // ustvari datoteko
    for(int i=0; i<konec.size(); i++){//nazaj pristejes kar si na zacetku odstel in IZPIS
    	izpis << konec[i]+minValue<<" ";
    }
}

int main(int argc, const char* argv[])
{
	string pot=argv[2]; // vnesi pot do datoteke s podatki
	if(argv[2]==NULL){
	    cout<<"Sort <opcija> <vhodna datoteka> "<<endl;
        cout<<"     <opcija>: 0 = Counting sort"<<endl;
        cout<<"               1 = Roman sort"<<endl;
        cout<<"     <vhodna datoteka>: pot do vhodne datoteke"<<endl;
        return 0;
    }
	int izbiraPrograma=atoi(argv[1]); // vnesi izbiro programa... atoi pretvori iz stringa v int

        ifstream file(pot.c_str());
        vector<int> numbers;
        int num;
        while(file >> num){
            numbers.push_back(num);
        }

        int minValue = *min_element(numbers.begin(), numbers.end());
        //cout<<"Min value: "<<minValue<<endl;
        int maxValue = (*max_element(numbers.begin(), numbers.end()))-minValue;

        for(int i=0; i<=numbers.size(); i++){ // vsem stevilom pristejemo najmanjse da se znebimo negativnih
            numbers[i] = numbers[i]-minValue;
        }

        vector<int> stevila;
        for(int i=0; i<maxValue+1; i++){ //napolni vektor z niclami
             stevila.push_back(0);
        }

        for(int i=0; i<=maxValue; i++){ //koliko je kerih stevil toliksno stevilo je na tistem indeksu
            stevila[i]=count (numbers.begin(), numbers.end(), i);
        }

        if(izbiraPrograma==0)
            countingSort(stevila, numbers, minValue);
        else if(izbiraPrograma==1)
            romanSort(stevila, minValue);
        else
            return 0;


  return 0;
}

