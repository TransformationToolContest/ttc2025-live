#!/usr/bin/python
"""
@author: Zsolt Kovari, Georg Hinkel

"""
import argparse
import os
import shutil
import subprocess
import logging
import sys
try:
    import ConfigParser
except ImportError:
    import configparser as ConfigParser
import json
from subprocess import CalledProcessError

BASE_DIRECTORY = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
print("Running benchmark with root directory " + BASE_DIRECTORY)

class JSONObject(object):
    def __init__(self, d):
        self.__dict__ = d


def build(conf, skip_tests=False):
    """
    Builds all solutions
    """
    for tool in conf.Tools:
        config = ConfigParser.ConfigParser()
        config.read(os.path.join(BASE_DIRECTORY, "solutions", tool, "solution.ini"))
        set_working_directory("solutions", tool)
        if skip_tests:
            subprocess.check_call(config.get('build', 'skipTests'), shell=True)
        else:
            subprocess.check_call(config.get('build', 'default'), shell=True)


def benchmark(conf):
    """
    Runs measurements
    """
    header = os.path.join(BASE_DIRECTORY, "output", "header.csv")
    result_file = os.path.join(BASE_DIRECTORY, "output", "output.csv")
    if os.path.exists(result_file):
        os.remove(result_file)
    shutil.copy(header, result_file)
    os.environ['Runs'] = str(conf.Runs)
    for r in range(0, conf.Runs):
        os.environ['RunIndex'] = str(r)
        for tool in conf.Tools:
            config = ConfigParser.ConfigParser()
            config.read(os.path.join(BASE_DIRECTORY, "solutions", tool, "solution.ini"))
            set_working_directory("solutions", tool)
            os.environ['Tool'] = tool
            for scenario in conf.Models:
                os.environ['Model'] = scenario.Name
                os.environ['Sequences'] = '0'
                full_model_path = os.path.abspath(os.path.join(BASE_DIRECTORY, "models", scenario.Name, scenario.Name + ".uvl"))
                for i in range(1, conf.MaxVersions):
                    if os.path.exists(os.path.join(BASE_DIRECTORY, "models", scenario.Name, scenario.Name + "_" + str(i).zfill(2) + ".uvl")):
                        os.environ['Sequences'] = str(i)
                        full_model_path = os.path.abspath(os.path.join(BASE_DIRECTORY, "models", scenario.Name, scenario.Name + "_01.uvl"))
                os.environ['ModelPath'] = full_model_path
                os.environ['ModelDirectory'] = os.path.abspath(os.path.join(BASE_DIRECTORY, "models", scenario.Name))
                print("Running benchmark: tool = " + tool + ", model = " + scenario.Name)
                try:
                    output = subprocess.check_output(config.get('run', 'default'), shell=True)
                    with open(result_file, "ab") as file:
                        file.write(output)
                except CalledProcessError as e:
                    print("Program exited with error" + repr(e))

def clean_dir(*path):
    dir = os.path.join(BASE_DIRECTORY, *path)
    if os.path.exists(dir):
        shutil.rmtree(dir)
    os.mkdir(dir)


def set_working_directory(*path):
    dir = os.path.join(BASE_DIRECTORY, *path)
    os.chdir(dir)

def _visualize(times, metric, unit, scale):
    import pandas as pd
    import numpy as np
    import matplotlib.pyplot as plt
    values = times[times.MetricName==metric]
    values['Values'] = values.MetricValue * scale
    models=np.unique(values.Model)
    for phase in np.unique(values.PhaseName):
        print("Printing diagram for phase = " + phase)
        phasedata = values[values.PhaseName==phase]
        results = pd.pivot_table(phasedata, values='Values', index=['Model'],columns=['Tool'])
        plot = results.plot()
        label = metric
        if unit is not None:
            label = label + " [" + unit + "]"
        plot.set_ylabel(label)
        plt.savefig(phase + ".pdf")

def visualize(conf):
    """
    Visualizes the benchmark results
    """
    clean_dir("diagrams")
    set_working_directory("diagrams")
    import pandas as pd
    data = pd.read_csv(os.path.join(BASE_DIRECTORY, 'output', 'output.csv'), sep=';')
    times = data[data.MetricName=='Time']
    _visualize(times, 'Time', 'ms', 0.000001)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("-b", "--build",
                        help="build the project",
                        action="store_true")
    parser.add_argument("-m", "--measure",
                        help="run the benchmark",
                        action="store_true")
    parser.add_argument("-s", "--skip-tests",
                        help="skip JUNIT tests",
                        action="store_true")
    parser.add_argument("-v", "--visualize",
                        help="create visualizations",
                        action="store_true")
    parser.add_argument("-t", "--test",
                        help="run test",
                        action="store_true")
    args = parser.parse_args()


    set_working_directory("config")
    with open("config.json", "r") as config_file:
        config = json.load(config_file, object_hook = JSONObject)

    if args.build:
        build(config, args.skip_tests)
    if args.measure:
        benchmark(config)
    if args.test:
        build(config, False)
    if args.visualize:
        visualize(config)

    # if there are no args, execute a full sequence
    # with the test and the visualization/reporting
    no_args = all(val==False for val in vars(args).values())
    if no_args:
        build(config, False)
        benchmark(config)
        visualize(config)