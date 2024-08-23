

# EFT-DMA-Radar-v2

## Description
EFT DMA Radar is a radar tool designed for Escape from Tarkov that provides real-time tracking of players and items on a 2D map. This fork of the radar adds ability to host a WebRadar alongside EFT DMA Radar. The webradar by default is hosted on localhost and can be accessed going to localhost in your browser. You can change the hostname/ip to your public ip or domain that points to your ip and share the link with your friends. For that to work you need to port forward port 80 in windows and your router.

## To Do
1. Add Container Selection to the WebRadar
2. Add Player Stats
3. Add Ammo Count to the selected player.
4. Add abilty to change player and loot colors.


## Known Issues
1. Webradar closes connection to the websocket when raid ends, simply click Restart Radar when new raid begins and refresh the website.
2. On first load you need to Refresh Loot for it to show on webradar.
3. Containers are for now disabled, as of the 0.15 update there has been some changes I need to address.
   
## Usage
1. Clone the repository.
2. Ensure all necessary dependencies are in place.
3. Compile the project.
4. Run the application.
5. Put in your hostname/ip and click Apply Hostname before starting the Web Server.
6. Your friends can select themselves from the list of players, this will set all the height markers according to their position.
7. Loot is marked with a green circle if it is at the same height as you, a triangle if it is above you and upside down triangle if it is below you.

## Dependencies
- FTD3XX.dll - https://ftdichip.com/drivers/d3xx-drivers/
- leechcore.dll, vmm.dll, dbghelp.dll, symsrv.dll and vcruntime140.dll - https://github.com/ufrisk/MemProcFS/releases

## Contact
For any inquiries or assistance, feel free to join the Discord server: https://discord.gg/jGSnTCekdx

## Note
Ensure all necessary files are properly included and referenced for the application to function correctly.

## Acknowledgments
This project builds upon the original work created by [Git link](https://github.com/6b45/eft-dma-radar-1) [UC Forum Thread](https://www.unknowncheats.me/forum/escape-from-tarkov/482418-2d-map-dma-radar-wip.html). I am not the original creator of this project; all credit for the initial concept and development goes to Lone. This version seeks to extend and enhance the original tool with updated functionalities and improvements. Big thanks to [Keegi](https://github.com/HuiTeab/) for continuing the project as long as he did & implementing many awesome features/functionality & MasterKeef for providing some maps to go with this

## Preview
![image](https://github.com/xx0m/EFT-DMA-Radar-v2/assets/63579245/9e55038f-8095-4680-9d3f-b14f44046276)
![image](https://github.com/xx0m/EFT-DMA-Radar-v2/assets/63579245/7a1f9f18-6373-4386-bd42-6666c04aa9f3)
![Screenshot 2024-08-22 225030](https://github.com/user-attachments/assets/8707f371-95a6-4b12-9dc9-600f36094128)
![image](https://github.com/xx0m/EFT-DMA-Radar-v2/assets/63579245/910ab73b-c633-4dc8-9753-a0f74b34b976)
![image](https://github.com/xx0m/EFT-DMA-Radar-v2/assets/63579245/9c7c5388-9e9b-4895-bd3a-2c8e137d17e6)
![Screenshot 2024-08-22 224828](https://github.com/user-attachments/assets/c50dae4b-ee1f-4970-b2d6-1a85638fcc70)
![Screenshot 2024-08-22 224804](https://github.com/user-attachments/assets/24984699-579d-48a5-92e9-6b66754d903e)
![Screenshot 2024-08-22 224751](https://github.com/user-attachments/assets/297c8847-4402-4434-a343-e4c50d842c23)
![Screenshot 2024-08-22 224741](https://github.com/user-attachments/assets/dd971770-cd78-48dd-85ec-ba29a3f42219)
