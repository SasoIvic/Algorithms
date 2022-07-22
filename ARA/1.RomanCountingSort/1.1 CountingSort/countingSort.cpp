#include <iostream>
#include <string>
#include <fstream>
#include <vector>
#include <algorithm>
using namespace std;

int main(int argc, const char* argv[])
{
        ifstream file( "primer_vhoda.txt" ); ///SPREMENI! PODATKE VSTAVLJAJ IZ KONZOLE
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
        //cout<<stevila[maxValue]<<" ";
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
        for(int i=0; i<konec.size(); i++){//nazaj pristejes kar si na zacetku odstel in IZPIS
            cout<<konec[i]+minValue<<" ";
        }

  return 0;
}

