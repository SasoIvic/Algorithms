#include <iostream>
#include <fstream>
#include <string>
#include <vector>

using namespace std;


int main()
{
	ifstream file("primer_vhoda.txt");
	vector<string> line;
	if (file.is_open()) {
		int i = 0;
		while (getline(file, line[i])){
			i++;
		}
		file.close();
	}

	cout << line[3];

    return 0;
}
