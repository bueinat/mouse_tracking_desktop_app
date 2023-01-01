# Mouse Tracking Application

Developed by Einat Buznach for Prof. Rafi Haddad's Lab.

For any questions or technical help: bueinat@gmail.com.

<!-- TOC -->

- [Mouse Tracking Application](#mouse-tracking-application)
  - [Requriements](#requriements)
  - [Installments](#installments)
    - [Git](#git)
    - [Anaconda](#anaconda)
    - [MongoDB](#mongodb)
    - [CUDA Tooklit](#cuda-tooklit)
    - [Pytorch](#pytorch)
    - [yolov5](#yolov5)
    - [DeepEthogram](#deepethogram)
  - [Train DeepEthogram Model](#train-deepethogram-model)
    - [Create a Model](#create-a-model)
    - [Train the Model](#train-the-model)
    - [Finalize Labels](#finalize-labels)
    - [For More Info](#for-more-info)

<!-- /TOC -->

## Requriements

In order to run this app you need to have GPU on your computer which supports CUDA.

## Installments

Install the followings, by clicking the links and following the instructions.
**Note:** in the future I wish to make all of this much simpler by writing a script which does all that.

### [Git](https://git-scm.com/download/win)

Follow the provided instructions.

### [Anaconda](https://www.anaconda.com/)

Make sure to add anaconda's directory to the path. You can do this in one of the following ways:

- add to path during the installation
- following the explanation [here](https://www.computerhope.com/issues/ch000549.htm#windows10)
- running the following command:

  ```bash
  setx /M path "%path%;path_to_anaconda"
  ```

### [MongoDB](https://www.mongodb.com/)

The app uses MongoDB as its database. Follow the instructions [here](https://www.mongodb.com/docs/manual/tutorial/install-mongodb-on-windows/) to install it.

In short, you first install latest version of [mongosh](https://www.mongodb.com/docs/mongodb-shell/install/#std-label-mdb-shell-install) (platform Windows 64-bit (8.1+) (MSI)), and then install [mongodb](https://www.mongodb.com/try/download/community?tck=docs_server). Just follow the instructions and make sure to install MongoDB as a service.

### [CUDA Tooklit](https://developer.nvidia.com/cuda-downloads)

You can install the network version. Detailed installation guidance is provided [here](https://docs.nvidia.com/cuda/cuda-installation-guide-microsoft-windows/index.html).

- after the installation is done you can see the installed version by `nvcc -V`.
- now you have some python installations to do. Run the following commands:

  ```bash
  conda install cuda -c nvidia
  pip install nvidia-pyindex
  pip install nvidia-cuda-runtime-cu11
  ```

### [Pytorch](https://pytorch.org/get-started/locally/)

Just pick the right setup for you. You should use pip (and not anaconda), and pick the newest cuda version available. Then copy the given line and change the cuda version according to the one you installed (I changed `116` -> `117`). You can try the tests presented later on in the page.

### [yolov5](https://github.com/ultralytics/yolov5)

If you installed `git`, all you have to do is run the following commands in your command line:

```bash
git clone https://github.com/ultralytics/yolov5  # clone
cd yolov5
pip install -r requirements.txt  # install
```

you have to make sure that where running those commands, you're in the right folder, where you want `yolov5` to be.
<!--comment: it's better if I do it myself and install it in the place I find better (i.e. ProgramData) -->

### [DeepEthogram](https://github.com/jbohnslav/deepethogram)

You simply install by typing:

```bash
pip install deepethogram
```

<!-- comment: I should add the moving files to a script which follows the installment (find it in the lab's computer)
                plus, download the pretrained models and add them to new ...-->

## Train DeepEthogram Model

The full instructions can be found [here](https://github.com/jbohnslav/deepethogram/blob/master/docs/using_gui.md), in `DeepEthogram`'s GitHub page. Here I will present the most important stuff, without much details.

### Create a Model
- Open any command line as you wish and type `DeepEthogram`. A window with the app will appear.
- Open a new project by pressing `file -> new project`, and pick a folder in which you want to have your project. It is recommended to have all the project in the same directory.
- Write a description as needed. Be careful with the features names - once you start training, adding a new features requires re-labeling the whole dataset. Write their names according to the right convension, which is fully presented in `DeepEthogram`'s GitHub page. The main points are:
  - the first behavior should be background (unnecessary when using the GUI)
  - names should be written in snake_case
  - no 'ing' suffix is needed
- Add videos by pressing `Video -> Add or open` (in order to add a single video) or `Video -> Add multiple` (in order to add multiple videos). The uploading process may take a while.
- Label the videos according to the instructions. The bare minimum is labelling 3 videos. Notice you don't have to do this a lot, but only once for each model (and I think you shouldn't have more than one). Don't forget to save the project once in a while.
- Once you finished labeling a video, press `finalize labels`. In order to go to the next video, you can open it again from the project.
- When creating a new model, you need to download the pretrained models from [here](https://drive.google.com/file/d/1ntIZVbOG1UAiFVlsAAuKEBEVCVevyets/view?usp=sharing) and put them in the `<project_name>/models` directory.

### Train the Model
Once you've created a model, you can train it. Before explaining about the training, I'd like to present the full workflow (pretty much taken from [here](https://colab.research.google.com/drive/1Nf9FU7FD77wgvbUFc608839v2jPYgDhd?usp=sharing#scrollTo=MDeo73x1dejq)):
- Train the flow generator. Only really needs to happen once if you start with >10 decently sized videos. You can re-train if you get lots more data, or if you change the recording conditions: this can include color, resolution, background, arena type, etc.
- Train the feature extractor
- Run inference using the feature extractor
- Train a sequence model
- Run inference using the sequence model.

You see we have one flow generator which needs to be trained once, and feature extractor and sequence model which are trained and make predictions. The training and predictions of them should happen any time you add a video.

The training of the flow generator should be done in the `DeepEthogram` GUI, and it might take quite long. You can also run the next steps in the GUI if you wish, but the mouse tracking app should do it for you as well.

To train the model you have to click on the `Train` button in the `FlowGenerator` box on the left side of the app, below the video info. Notice you can pick an existing model to train on if you wish.

![image](https://user-images.githubusercontent.com/62245924/209649536-61508df4-e882-461d-a95d-45696128bc65.png)

If you want to do the whole training on the GUI, you should look into [this link](https://github.com/jbohnslav/deepethogram/blob/master/docs/using_gui.md) again.
 
 ### Finalize Labels
 Once you completed the training, inferring and making predictions (doesn't matter if you used `DeepEthogram` or the mouse tracking app), there would be a prediction file for each video. You can use the same system of labelling as before, fix the predictions to get the real labels and finalize them (just as you did when creating the model).
 
### For More Info
as I mentioned, In [`DeepEthogram`'s GitHub page](https://github.com/jbohnslav/deepethogram/blob/master/docs/using_gui.md) you can find the full instructions for using the GUI. Plus, you can use [this](https://colab.research.google.com/drive/1Nf9FU7FD77wgvbUFc608839v2jPYgDhd?usp=sharing) uploaded notebook which has more details about the training process and how it's done.

## The App
Due to some problems, the app is not deployed, thus you have to open it directly from Visual Studio. Click the icon on the desktop:
![image](https://user-images.githubusercontent.com/62245924/210171805-b6ed20c1-54e4-46c3-9caf-832933da7703.png)

A window of the soultion will open. In order to run the app you press the start button in the upper row:
![image](https://user-images.githubusercontent.com/62245924/210171843-40211fd1-d481-4e19-8dd2-7670eded1fa7.png)

you will see the following image:
![image](https://user-images.githubusercontent.com/62245924/210171898-7643d04c-a7da-4975-9750-197c6222db45.png)

the window which opened is the app, and the output section below shows the output of the system. You can see if there are any errors or problems.

### Setting
if you press the setting button in the bottom you can change the settings of the project:
![image](https://user-images.githubusercontent.com/62245924/210171949-2251c430-0e6c-4fa0-a6e8-ebec835b3f24.png)

* **working path:** the path whose folders will be shown in the app
* **deepethogram path:** the path of the deepethogram project that should be used
* **python path:** path of python in this computer. Used in case you want to change the python version
* **connection string and database name:** related to the database. shouldn't be relevant
* **file types and videos types:** the relevant filetypes you want to use
* **marker size in plot:** this is the default size in the plots, you can change this any time
* **override:** whether you want to delete old files when introducing new ones, or keep the ones from before

You can reset to default in case you need. You can also return to the settings section from the app itself.

### Videos
![image](https://user-images.githubusercontent.com/62245924/210172125-11c92a11-546e-46e2-824e-484295ead488.png)


### Analyze
![image](https://user-images.githubusercontent.com/62245924/210172161-77de33cc-e73f-427e-8fad-3e04eca3d955.png)


