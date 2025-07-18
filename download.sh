
printf "\033c\033[43;30m\n"

mkdir -p $HOME/android-sdk/cmdline-tools
cd $HOME/android-sdk

# Baixar os command line tools (versão mais recente)
wget https://dl.google.com/android/repository/commandlinetools-linux-10406996_latest.zip -O cmdline-tools.zip

# Descompactar
unzip cmdline-tools.zip
mv cmdline-tools cmdline-tools/latest
