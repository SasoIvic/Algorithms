#include "MaxFlow.h"

bool Graph::BFS(int s, int t){
	for (int i=0; i < num_points; i++){	//set all points to undiscovered
		color[i] = WHITE;
	}
	
	int u;
	color[s] = GRAY;
	distance[s] = 0;
	Q.push(s);
	while (Q.size() != 0){	//while queue not empty
		u = Q.front();	//dequeue
		Q.pop();
		for(int i = 0; i < V.size(); i++){	//adjacent
			for(int j = 0; j < V[i].size(); j++){
				int v=V[i][j];
				if (color[v] == WHITE && edges[u][v].residual > 0){//if not visited & residual capacity > 0
					color[v] = GRAY;
					//cout << v << ")Discovered" << endl; // Debug--- Show discovered vertices
					distance[v]=distance[u]+1;
					parent[v] = u;
					Q.push(v);	//enqueue
				}
				color[u] = BLACK;
			}
		}
	}		
	if (color[t] == BLACK){
		return true;
	}else{
		return false;
	}
}

void Graph::resize(int n){//Resize edge array to size n
	Edge **tmp = new Edge*[arr_size];		//Copy old array
	for (int s = 0; s < arr_size; s++){
		for(int m = 0; m < arr_size; m++){
			tmp[s] = new Edge[arr_size];
			tmp[s][m] = edges[s][m];
		}
	}
	
	edges = new Edge*[n];		//Create and initialize old array
		for (int i = 0; i < n; i++){
			for(int j = 0; j < n; j++){
				edges[i] = new Edge[n];
				edges[i][j].residual = -1;	
			}
		}	
	
	for (int a = 0; a < arr_size; a++){	//fill new array with tmp array
		for(int b = 0; b < arr_size; b++){
			edges[a][b] = tmp[a][b];
		}
	}
	arr_size = n;
	delete tmp;
}


void Graph::addEdge(int u, int v, int cap){//directed edge
	edges[u][v].capacity = cap;
	edges[u][v].residual = cap;
	V[u].push_back(v);
	num_points++;
	color.insert(make_pair(u, WHITE));
	color.insert(make_pair(v, WHITE));
	//cout <<u<<"->"<<v<<": "<< edges[u][v].residual<< "/"<<edges[u][v].capacity<< endl; //Debug---show edge residual/capacity
}

//Ford-Fulkerson Algorithm
int Graph::findMaxFlow(int s, int t){	//returns max flow from s to t 
	int max_flow = 0;
	int u;
	cout <<"All Edges (Residual Capacity/Capacity):" << endl;
	while (BFS(s,t)==true){
		int path_flow = INT_MAX;
		for (int i = t; i !=s; i = parent[i]){
			u =parent[i];
			if (path_flow > edges[u][i].residual){
				path_flow = edges[u][i].residual;
			}
			
		}
		for (int i=t; i!=s; i = parent[i]){	//update residual capacities
			u = parent[i];
			edges[u][i].residual -= path_flow;
			edges[i][u].residual += path_flow;
			
			cout <<u<<"->"<<i<<": "<< edges[u][i].residual<< "/"<<edges[u][i].capacity<< endl;	//Debug---show edge residual/capacity
		}
		max_flow += path_flow;
	}
	cout <<"Path with Maximum Capacity from s to t:" << endl;
	for (int i=t; i!=s; i = parent[i]){	//print path
			u = parent[i];
			
			cout <<u<<"->"<<i<<": "<< edges[u][i].residual<< "/"<<edges[u][i].capacity<< endl;	//show edge residual/capacity
		}
	return max_flow;
}
	