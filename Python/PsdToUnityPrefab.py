from getopt import getopt
import sys
import os
import chardet
from psd_tools import PSDImage

from xml.dom.minidom import Document


screenWidth = 2340
screenHeight = 1080

inputFileName = ''
expotFolder = ''

def calcPos(layer):
    # 坐标原点左上角
    imageWidth = layer.size[0]
    imageHeight = layer.size[1]
    imageTop = layer.top
    imageLeft = layer.left
    imageBottom = layer.bottom
    imageRight = layer.right
    # image 中心坐标
    imageX = imageLeft + imageWidth / 2 
    imageY = imageTop +  imageHeight / 2
    # 将坐标原点转换成 中心点
    imageNewX = int(screenWidth / 2 - imageX) * -1
    imageNewY = int(screenHeight / 2 - imageY)
    return imageNewX, imageNewY

def checkLayer(doc, layer, element):
    if layer.is_group():
        group = doc.createElement('group')
        imageNewX, imageNewY = calcPos(layer)
        group.setAttribute('name', str(layer.name))
        group.setAttribute('size', str(layer.size))
        group.setAttribute('pos', str((imageNewX, imageNewY)))
        group.setAttribute('type', str(layer.kind))
        element.appendChild(group)
        for child in layer:
            checkLayer(doc, child, group)
    else:
        if not layer.is_visible():
            return
        imageNewX, imageNewY = calcPos(layer)
        nodeLayer = doc.createElement('Image')
        nodeLayer.setAttribute('name', layer.name)
        layerText = ''
        print(f'Name:{layer.name},  Kind:{layer.kind}')
        if layer.kind != 'type' and layer.kind != 'brightnesscontrast' and layer.kind != 'huesaturation':
            layer_image = layer.composite()
            path = f'{expotFolder}\\{layer.name}.png'
            layer_image.save(path)
        elif layer.kind == 'type':
            layerText = layer.text
        
        # layer_image.save(f'{layer.name}.png')
        nodeLayer.setAttribute('size', str(layer.size))
        #nodeLayer.setAttribute('pos', str((imageLeft, imageTop, imageRight, imageBottom)))
        nodeLayer.setAttribute('pos', str((imageNewX, imageNewY)))
        nodeLayer.setAttribute('type', str(layer.kind))
        nodeLayer.setAttribute('text', layerText)
        # print(f'layer text: {layerText}')   
        element.appendChild(nodeLayer)



def expotPSD(fullpath):

    global screenWidth
    global screenHeight
    print(f'PSD Path: {fullpath}')
    psd = PSDImage.open(f'{fullpath}')
    #psd.composite().save('example.png')

    doc = Document()
    root = doc.createElement('root')
    root.setAttribute('name', str(inputFileName))
    root.setAttribute('size', str(psd.size))
    root.setAttribute('pos', str((0, 0)))
    screenWidth = psd.size[0]
    screenHeight = psd.size[1]

    doc.appendChild(root)
    for layer in psd:
        checkLayer(doc, layer, root)


    filename = f'{expotFolder}\\{inputFileName}.xml'
    f = open(filename, 'w', encoding='utf8')
    doc.writexml(f, indent='\t', addindent='\t', newl='\n')#, encoding='utf-8')
    f.close()


def main():
    # 脚本名 sys.argv[0]
    opts, args = getopt(sys.argv[1:], 'hi:o:f:')
    input_fullpath = ''
    global expotFolder
    global inputFileName
    for op, value in opts:
        if op == '-i':
            input_fullpath = value
        elif op == '-o':
            expotFolder = value
        elif op == '-f':
            inputFileName = value 
    
    expotFolder = f'{expotFolder}\\{inputFileName}'

    if not os.path.exists(expotFolder):
        os.makedirs(expotFolder)
        print(f'create {expotFolder} folder success')

    print(inputFileName)
    print(expotFolder)
    print(input_fullpath)
    
    expotPSD(input_fullpath)
    print(f'export PSD to Image Success, path:{expotFolder}')


if __name__ == "__main__":
    print('Hello World!')
    main()
