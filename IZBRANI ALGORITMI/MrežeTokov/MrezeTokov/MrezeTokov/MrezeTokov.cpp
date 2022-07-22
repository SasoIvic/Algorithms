#include <iostream>
#include <chrono>
#include <time.h> 
#include<map>
#include<vector>
#include<iostream>
#include<queue>
#include<limits.h>
#include <fstream>
#include <sstream>
#include <algorithm>

using namespace std;


class Graph {

public:
	struct Connection;
	int num_points;

	enum status {
		UNVISITED,
		INPROGRESS,
		DONE
	};

	struct Node {
		int value;
		Node* parent;
		vector<Node*> neighbours;
		vector<Connection*> distances;
		status status;
	};

	struct Connection {
		Node* node;
		int distance;
		int flow;
	};

	queue<Node*> inProgressNodes;
	vector<Node> nodes;

	Graph(int m) {
		num_points = m;
	}

	bool BFS(Node* s, Node* e) {

		//set all points to unvisited
		for (auto& n : nodes)
			n.status = UNVISITED;

		s->status = INPROGRESS;
		while (!inProgressNodes.empty()) { inProgressNodes.pop(); }
		inProgressNodes.push(s);

		//while queue not empty
		while (inProgressNodes.size() != 0) {

			Node* u = inProgressNodes.front();	
			inProgressNodes.pop();

			if (u->value == e->value)
				return true;

			for (auto& n : nodes) {
				for (auto& n_u : u->neighbours) {
					if (n.value == n_u->value) {
						n_u = &n;
					}
				}
			}

			//go trough all neighbour nodes
			for (auto& n : u->neighbours) {
				if (u->distances.size()-1 >= n->value) {
					if (n->status == UNVISITED && (u->distances[n->value]->distance - u->distances[n->value]->flow) > 0) {
						n->status = INPROGRESS;

						n->parent = u;
						inProgressNodes.push(n);
					}
				}
			}
			u->status = DONE;

		}
		if (e->status == DONE) return true;
		else return false;

	}

	void addEdge(int u, int v, int cap, int nodesNum) {

		Node neighbour = Node();
		bool neighbourExists = false;
		bool nodeExists = false;

		for (auto& n : nodes) {
			if (n.value == v) {
				neighbour = n;
				neighbourExists = true;
				break;
			}
		}

		if (!neighbourExists) {

			Node node = Node();
			node.value = v;
			node.status = UNVISITED;

			for (int i = 0; i <= nodesNum; i++)
				node.distances.push_back(0);

			neighbour = node;
			nodes.push_back(node);
		}

		Connection* conn = new Connection();
		conn->node = &neighbour;
		conn->flow = 0;
		conn->distance = cap;


		for (auto& n : nodes) {
			if (n.value == u) {

				Node* tmp = new Node();
				tmp->distances = neighbour.distances;
				tmp->value = neighbour.value;
				tmp->parent = neighbour.parent;
				tmp->status = neighbour.status;
				tmp->distances = neighbour.distances;

				n.neighbours.push_back(tmp);
				n.distances[v] = conn;

				nodeExists = true;
				break;
			}
		}

		if (!nodeExists) {

			Node node = Node();
			Node* tmp = new Node();

			tmp->distances = neighbour.distances;
			tmp->value = neighbour.value;
			tmp->parent = neighbour.parent;
			tmp->status = neighbour.status;
			tmp->distances = neighbour.distances;

			node.value = u;
			node.neighbours.push_back(tmp);
			node.status = Graph::UNVISITED;

			for (int i = 0; i <= nodesNum; i++)
				node.distances.push_back(0);

			node.distances[v] = conn;
			nodes.push_back(node);
		}

	}

	int findMaxFlow(Node* s, Node* e) {
		
		Node* u;
		int max_flow = 0;

		while (BFS(s, e) == true) {

			int path_flow = INT_MAX;

			for (auto i = e; i != s; i = i->parent) {
				u = i->parent;
				//cout << u->value << "->" << i->value << ": " << u->distances[i->value]->flow << "/" << u->distances[i->value]->distance << endl;
			}

			//go trough path (all parents)
			for (auto i = e; i != s; i = i->parent) {
				u = i->parent;

				if (path_flow > (u->distances[i->value]->distance - u->distances[i->value]->flow))
					path_flow = (u->distances[i->value]->distance - u->distances[i->value]->flow);
			}

			//update capacities
			for (auto i = e; i != s; i = i->parent) {
				u = i->parent;
				u->distances[i->value]->flow += path_flow;

				//if reverse connection exists
				if (count(i->neighbours.begin(), i->neighbours.end(), u))
					i->distances[u->value]->flow -= path_flow;
			}
			max_flow += path_flow;

			//cout << "path flow: " << path_flow << endl;
			//cout << "max flow: " << max_flow << endl;
		}


		//Print final path
		/*cout << "Path with Maximum Capacity from s to t:" << endl;
		if (e->parent != NULL) {
			for (auto& i = e; i != s; i = i->parent) {
				u = i->parent;
				cout << u->value << "->" << i->value << ": " << (u->distances[i->value]->distance - u->distances[i->value]->flow) << "/" << u->distances[i->value]->distance << endl;
			}
		}
		else {
			cout << "Path not found." << endl;
		}


		drawGraph();*/

		return max_flow;
	}


	void drawGraph() {
		ofstream MyFile("GraphImage.dot");
		MyFile << "digraph G {" << endl;

		for (auto node : nodes) { 
			for (auto neighbour : node.neighbours) {
				if (neighbour != NULL) {
					MyFile << node.value << " -> " << neighbour->value << "[label=\"(" << node.distances[neighbour->value]->flow << "/" << node.distances[neighbour->value]->distance << ")\"];" << endl;
				}
			}
		}

		MyFile << "}";
		MyFile.close();

		string command = "dot -Tpng GraphImage.dot -o GraphImage.png";
		system(command.c_str());
	}
};

void readFile() {

	auto start = std::chrono::system_clock::now();
	auto end = std::chrono::system_clock::now();
	std::chrono::duration<double> elapsed_seconds;

	int n, m; //št vozlišč in št povezav
	string n1, n2, p;

	//Read file
	ifstream file("graf.txt");
	if (file.is_open()) {

		file >> n >> m;

		Graph graph(m);

		//addEdge(1st point, 2nd point, capacity, nodesNum)
		while (file >> n1 >> n2 >> p) {
			graph.addEdge(stoi(n1), stoi(n2), stoi(p), n);
		}
		file.close();

		//Začetno in končno vozlišče
		int startNode, endNode;
		cout << "Vnesite zacetno in koncno vozlisce: \n";
		cin >> startNode >> endNode;
		cout << endl;

		Graph::Node* s = NULL;
		Graph::Node* e = NULL;

		for (auto& n1 : graph.nodes) {
			if (n1.value == startNode) {
				s = &n1;
				break;
			}
		}

		for (auto& n2 : graph.nodes) {
			if (n2.value == endNode) {
				e = &n2;
				break;
			}
		}

		start = std::chrono::system_clock::now();

		//find max flow from startNode to endNode
		cout << "The Maximum Flow is: " << graph.findMaxFlow(s, e) << endl;

		end = std::chrono::system_clock::now();
		elapsed_seconds = end - start;
		std::cout << "elapsed time: " << elapsed_seconds.count() << "s\n";
	}
}

std::chrono::duration<double> readFileWithRandom(string fileName, int i) {

	auto start = std::chrono::system_clock::now();
	auto end = std::chrono::system_clock::now();
	std::chrono::duration<double> elapsed_seconds;

	int n, m; //št vozlišč in št povezav
	string n1, n2, p;

	//Read file
	ifstream file(fileName);
	if (file.is_open()) {

		file >> n >> m;

		Graph graph(m);

		//addEdge(1st point, 2nd point, capacity, nodesNum)
		while (file >> n1 >> n2 >> p) {
			int a = -1;
			if (p == "w" || p == "\v") {
				a = i;
			}

			if(a == -1)
				graph.addEdge(stoi(n1), stoi(n2), stoi(p), n);
			else
				graph.addEdge(stoi(n1), stoi(n2), a, n);
		}
		file.close();

		//Začetno in končno vozlišče
		int startNode = 0, endNode = 3;

		Graph::Node* s = NULL;
		Graph::Node* e = NULL;

		for (auto& n1 : graph.nodes) {
			if (n1.value == startNode) {
				s = &n1;
				break;
			}
		}

		for (auto& n2 : graph.nodes) {
			if (n2.value == endNode) {
				e = &n2;
				break;
			}
		}

		start = std::chrono::system_clock::now();

		//find max flow from startNode to endNode
		graph.findMaxFlow(s, e);

		end = std::chrono::system_clock::now();
		elapsed_seconds = end - start;

		return elapsed_seconds;
	}
}

void getStatistics(string fileName) {
	int repeats = 1000;

	cout << "Average time: " << endl;

	for (int i = 10; i <= 1000000; i += 10000) {

		double averageTime = 0;

		for (int j = 0; j < repeats; j++) {

			std::chrono::duration<double> time = readFileWithRandom(fileName, i);
			averageTime += time.count()* 1000; //mili seconds
		}

		cout << averageTime / repeats << endl;
	}

}


int main() {

	int ch;
	cout << "Osnovni primer (1) ali Testiranje zahtevnosti glede na vhod (2) \n";
	cin >> ch;

	if (ch == 1) {
		readFile();
	}
	else {
		string fileName;
		cout << "Vnesite ime datoteke: ";
		cin >> fileName;

		getStatistics(fileName);
	}



}