# Mouse Tracking Application

Developed by Einat Buznach for Prof. Rafi Haddad's Lab.

<!-- For any questions or technical help: @bueinat. -->

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

- Open any command line as you wish and type `DeepEthogram`. A window with the app will appear.
- Open a new project by pressing `file -> new project`, and pick a folder in which you want to have your project. It is recommended to have all the project in the same directory.
- Write a description as needed. Be careful with the features names - once you start training, adding a new features requires re-labeling the whole dataset. Write their names according to the right convension (presented in `DeepEthogram`'s GitHub page).
- Add videos by pressing `Video -> Add or open` (in order to add a single video) or `Video -> Add multiple` (in order to add multiple videos). The uploading process may take a while.
- Label the videos according to the instructions. The bare minimum is labelling 3 videos. Notice you don't have to do this a lot, but only once for each model (and I think you shouldn't have more than one). Don't forget to save the project once in a while.
- Once you finished labeling a video, press `finalize labels`. In order to go to the next video, you can open it again from the project.
- When creating a new model, you need to download the pretrained models from [here](https://drive.google.com/file/d/1ntIZVbOG1UAiFVlsAAuKEBEVCVevyets/view?usp=sharing) and put them in the `<project_name>/models` directory.
