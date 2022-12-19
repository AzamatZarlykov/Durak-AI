import sys
import subprocess

def parse_param(param):
	p = param.split('=')

	name = p[0]
	val_arr = p[1][1:-1] #10/100/500

	values = []
	for v in val_arr.split('/'):
		values.append(v)

	return (name, values)

def parse(agent_param, param_buffer):
	parameters = agent_param.split(',')

	for pair in parameters:
		(name, values) = parse_param(pair)
		param_buffer[name] = values

def run_mcts(param_buffer, keys, open_state):
	limits = param_buffer[keys[0]]
	cs = param_buffer[keys[1]]
	simulations = param_buffer[keys[2]]

	
	
	opp = param_buffer["opponent"]
	games = param_buffer["total-games"]

	for limit in limits:
		for c in cs:
			for simulation in simulations:
				command = ["dotnet", "run", 
							f"-ai1=mcts:limit={limit},c={c},simulation={simulation}",
							f"-ai2={opp}", f"-total_games={games}", "-start_rank=6", "-log"]

				if not open_state:
					if "samples" not in param_buffer:
						raise Exception("'samples' parameter is not provided in the closed world")
					
					# in the closed-world update the -ai1 parameter by adding samples parameter
					samples = param_buffer[keys[3]]
					for sample in samples:
						command[2] += f",samples={sample}"
						# run the command
						subprocess.run(command)

				else:
					command.append("-open_world")
					# run the command
					#subprocess.run(command)
					print(" ".join(command))
				



def main(agent_name, agent_param, opponent, total_games, open_state):
	# store the parameters and their values to dict
	param_buffer = {}
	parse(agent_param, param_buffer)
	param_buffer["opponent"] = opponent
	param_buffer["total-games"] = total_games

	keys = list(param_buffer.keys())
	print(keys)
	if agent_name == "mcts":
		# run the game with all the possible values for parameters to find the best combination of params
		run_mcts(param_buffer, keys, open_state)
	elif agent_name == "minimax":
		pass
	else:
		raise Exception("Unknown agent: {agent_name}")



if __name__ == '__main__':
	# parameters (strict order):
	# open-world: 
	#	(1) mcts:limit=[10/100/500],c=[0/1/1.41],simulations=[greedy/random] greedy 100 open_world
	#	(2) minimax:depth=[2/4/6/8],eval=[basic/playout] greedy 100 open_world
	# closed-world: 
	# (must provide samples param)
	#	(1) mcts:limit=[10/100/500],c=[0/1/1.41],simulations=[greedy/random],samples=[20/30] greedy	100
	#	(2) minimax:depth=[2/4/6/8],eval=[basic/playout],samples=[20/30] greedy 100

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