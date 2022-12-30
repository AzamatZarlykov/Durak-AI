import os
import sys
import numpy as np
import subprocess

def parse_param(param):
	p = param.split('=')
	name = p[0]
	val_arr = p[1][1:-1] #10/100/500

	values = []
	for v in val_arr.split('/'):
		values.append(int(v)) if v.isdigit() else values.append(v)

	return (name, values)

def parse(agent_param, param_buffer):
	parameters = agent_param.split(',')

	for pair in parameters:
		(name, values) = parse_param(pair)
		param_buffer[name] = values

def run_mcts(param_buffer, keys, open_state):
	def change_parameter(command, open):
		if open:
			s = command[-1].split(',')[:-1]
			s.append(f"limit={limit}")
			command[-1] = ','.join(s)
			command.append("-open_world")
		else:
			s = command[-1].split(',')[:-2]
			s.append(f"samples={sample}")
			s.append(f"limit={limit//sample}")
			command[-1] =  ','.join(s)

	limits, cs, simulations = param_buffer[keys[0]], param_buffer[keys[1]], param_buffer[keys[2]]
	opp, games = param_buffer["opponent"], param_buffer["total-games"]

	for limit in range(limits[0], limits[1], limits[2]):
		for c in np.arange(float(cs[0]), float(cs[1]), float(cs[2])):
			for simulation in simulations:
				command = ["dotnet", "run", "-start_rank=6", "-config", f"-total_games={games}",
							f"-ai2={opp}", f"-ai1=mcts:c={c},simulation={simulation},"]

				if not open_state:
					# in the closed-world update the -ai1 parameter by adding samples parameter
					samples = param_buffer[keys[3]]
					for sample in range(samples[0], samples[1], samples[2]):
						change_parameter(command, False)
						# run the command
						print(" ".join(command))
						subprocess.run(command, cwd="../")
				else:
					change_parameter(command, True)
					# run the command
					print(" ".join(command))
					subprocess.run(command, cwd="../")



def run_minimax(param_buffer, keys, open_state):
	depths, heuristics = param_buffer[keys[0]], param_buffer[keys[1]]
	opp, games = param_buffer["opponent"], param_buffer["total-games"]

	for depth in range(depths[0], depths[1], depths[2]):
		for heuristic in heuristics:
			command = ["dotnet", "run", 
						f"-ai1=minimax:depth={depth},eval={heuristic}",
						f"-ai2={opp}", f"-total_games={games}", "-start_rank=6", "-config"]

			if not open_state:
				command[2] += f",samples=0"

				# in the closed-world update the -ai1 parameter by adding samples parameter
				samples = param_buffer[keys[2]]
				for sample in range(samples[0], samples[1], samples[2]):
					s = command[2].split(',')[:-1]
					s.append(f"samples={sample}")
					command[2] =  ','.join(s)
					# run the command
					print(" ".join(command))
					subprocess.run(command, cwd="../") 
			else:
				command.append("-open_world")
				# run the command
				print(" ".join(command))
				subprocess.run(command, cwd="../")

def error_checking(agent_name, open_state, keys):
	# check samples for open world
	if open_state and "samples" in keys:
		raise Exception(f"'sample' parameter must not be specified in the open world")
	# check samples for closed world
	if not open_state and "samples" not in keys:
		raise Exception(f"'samples' parameter must be specified in the closed world" )

	# check mcts if all parameters are provided: e.g 3 in the open world (limit,c,simulations)
	if agent_name == "mcts" and ("limit" not in keys or "c" not in keys or "simulations" not in keys):
		raise Exception(f"not all parameters are specified in {agent_name}")

	# check minimax if all parameters are provided: e.g 3 in the open world (depth,eval)
	if agent_name == "minimax" and ("depth" not in keys or "eval" not in keys):
		raise Exception(f"not all parameters are specified in Minimax")

def separate_results(name, param_buffer, keys):
	if os.path.exists("ParamLogs/result.txt"):
		# instead of overwriting to a file, make a clear seperation 
		# between the previous and current experiment
		with open("ParamLogs/result.txt",'a') as f:
			f.write("\n===========================\n\n")

def main(agent_name, agent_param, opponent, total_games, open_state):
	# store the parameters and their values to dict
	param_buffer = {}
	parse(agent_param, param_buffer)
	param_buffer["opponent"] = opponent
	param_buffer["total-games"] = total_games

	keys = list(param_buffer.keys())

	error_checking(agent_name, open_state, keys)  
	separate_results(agent_name, param_buffer, keys)

	print(keys)
	if agent_name == "mcts":
		# run the game with all the possible values for parameters to find the best combination of params
		run_mcts(param_buffer, keys, open_state)
	elif agent_name == "minimax":
		run_minimax(param_buffer, keys, open_state)
	else:
		raise Exception("Unknown agent: {agent_name}")



if __name__ == '__main__':
	# parameters (strict order):
	# [min/max/step value]
	# open-world: 
	#	(1) mcts:limit=[100/1000/100],c=[0.6/2.4/0.2],simulations=[greedy/random] greedy 100 open_world
	#	(2) minimax:depth=[2/8/2],eval=[basic/playout] greedy 100 open_world
	# closed-world: 
	# (must provide samples param)
	#	(1) mcts:limit=[100/1000/100],c=[0.6/2.4/0.2],simulations=[greedy/random],samples=[20/30/10] greedy	100
	#	(2) minimax:depth=[2/8/2],eval=[basic/playout],samples=[20/30/10] greedy 100

	n = len(sys.argv)

	if n != 4 and n != 5: 
		raise Exception(f"Incorrect number of parameters: {n}. Has to be 4 or 5")

	parameter = sys.argv[1].split(':')

	agent_name = parameter[0]  
	agent_param = parameter[1]
	opponent = sys.argv[2]
	total_games = sys.argv[3]
	open_state = False

	if len(sys.argv) == 5 and sys.argv[4] == "open_world":
		open_state = True

	main(agent_name, agent_param, opponent, total_games, open_state)