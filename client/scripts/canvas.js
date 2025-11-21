const titleInput = document.getElementById('drawing-title')
const form = document.querySelector('form')
const errors = document.getElementsByClassName('error')

titleInput.addEventListener('input', () => checkTitleValidity())

function checkTitleValidity() {
    if (titleInput.validity.valid) {
        errors[0].innerHTML = ''
        return true
    } else {
        errors[0].textContent = 'Title required'
        return false
    }
}

form.addEventListener('submit', (event) => {
    if(checkTitleValidity()) {
        saveDrawing()
    }
    event.preventDefault()
})

async function saveDrawing() {
    const jwt = sessionStorage.getItem('jwt_token')
    let id

    if (jwt) {
        const payload = jwt.split('.')[1]
        id = JSON.parse(atob(payload)).sid
    } else {
        alert('Something went wrong. Please log in again')
        window.location.replace('index.html')
        return
    }
 
    try {
        const response = await fetch(`http://localhost:5043/draweb-api/users/${id}/drawings`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${jwt}`
            },
            body: JSON.stringify({
                title: titleInput.value,
                svg: svgCanvas.outerHTML
            })
        })

        const responseCode = response.status
        if (responseCode == 200) {
            errors[0].innerHTML = ''
            alert('Drawing successfully saved')
        } else if (responseCode == 401) {
            alert('Please login again')
            window.location.replace('index.html')
        } else {
            alert('Something went wrong. Try again later')
        }
    } catch (e) {
        console.log(e)
        alert('Something went wrong')
    }
}


const toolbarShapes = document.getElementsByClassName('toolbar-shape-button')
for (let i = 0; i < toolbarShapes.length; i++) {
    toolbarShapes[i].addEventListener('click', () => {
        toolbarShapes[i].classList.toggle('is-active')
        const activeShapes = document.getElementsByClassName('is-active')
        for (let j = 0; j < activeShapes.length; j++ ) {
            if (activeShapes[j].id !== toolbarShapes[i].id) {
                activeShapes[j].classList.toggle('is-active')
            }
        }
    })
}

const svgNS = "http://www.w3.org/2000/svg"
const svgCanvas = document.getElementById('canvas')
const drawingCoordinates = {
    x1: null,
    y1: null,
    x2: null,
    y2: null,
    addStartingCoordinates: (event) => {
        const rect = svgCanvas.getBoundingClientRect();
        const mouseXPosition = event.clientX - rect.left;
        const mouseYPosition = event.clientY - rect.top;
        drawingCoordinates.x1 = mouseXPosition
        drawingCoordinates.y1 = mouseYPosition
    },
    addFinalCoordinates: (event) => {
        const rect = svgCanvas.getBoundingClientRect();
        const mouseXPosition = event.clientX - rect.left;
        const mouseYPosition = event.clientY - rect.top;
        drawingCoordinates.x2 = mouseXPosition
        drawingCoordinates.y2 = mouseYPosition
    },
    clearCoordinates: () => {
        drawingCoordinates.x1 = null
        drawingCoordinates.y1 = null
        drawingCoordinates.x2 = null
        drawingCoordinates.y2 = null
    }
}
const movingCoordinates = {
    drawing: null,
    initialClickX: null,
    initialClickY: null,
    lastClickX: null,
    lastClickY: null,
    addInitialCoordinates: (event) => {
        const rect = svgCanvas.getBoundingClientRect();
        const mouseXPosition = event.clientX - rect.left;
        const mouseYPosition = event.clientY - rect.top;
        movingCoordinates.initialClickX = mouseXPosition
        movingCoordinates.initialClickY = mouseYPosition
    },
    addLastCoordinates: (event) => {
        const rect = svgCanvas.getBoundingClientRect();
        const mouseXPosition = event.clientX - rect.left;
        const mouseYPosition = event.clientY - rect.top;
        movingCoordinates.lastClickX = mouseXPosition
        movingCoordinates.lastClickY = mouseYPosition
    },
    clearCoordinates: () => {
        movingCoordinates.initialClickX = null
        movingCoordinates.initialClickY = null
        movingCoordinates.lastClickX = null
        movingCoordinates.lastClickY = null
        movingCoordinates.drawing = null
    }
}
let isDrawing = false
let isMoving = false
let shapesCount = 0

svgCanvas.addEventListener('mousedown', (event) => {
    if (event.target.id === svgCanvas.id && document.getElementsByClassName('is-active')[0]) {
        isDrawing = true
        drawingCoordinates.addStartingCoordinates(event)
        drawingCoordinates.addFinalCoordinates(event)
        drawShape()
    }
})

svgCanvas.addEventListener('mousemove', (event) => {
    if(isDrawing) {
        svgCanvas.removeChild(svgCanvas.lastChild)
        drawingCoordinates.addFinalCoordinates(event)
        drawShape(event)
    } else if (isMoving) {
        movingCoordinates.addLastCoordinates(event)
        drawShape()
    }
})

svgCanvas.addEventListener('mouseup', (event) => {
    if (isDrawing) {
        isDrawing = false
        drawingCoordinates.clearCoordinates()

        const drownShape = svgCanvas.lastChild
        drownShape.setAttribute('id', shapesCount)
        shapesCount++
        drownShape.addEventListener('mousedown', (mousedownEvent) => {
            isMoving = true
            movingCoordinates.drawing = drownShape
            movingCoordinates.addInitialCoordinates(mousedownEvent)
        })
    } else if (isMoving) {
        isMoving = false
        movingCoordinates.clearCoordinates()
    }
})

function drawShape() {
    if (isDrawing) {
        const activeShape = document.getElementsByClassName('is-active')[0]
        let newShape
        switch(activeShape.id) {
            case 'line-shape':
                newShape = document.createElementNS(svgNS, 'line')
                newShape.setAttribute('x1', drawingCoordinates.x1)
                newShape.setAttribute('y1', drawingCoordinates.y1)
                newShape.setAttribute('x2', drawingCoordinates.x2)
                newShape.setAttribute('y2', drawingCoordinates.y2)
                break;
            case 'ellipse-shape':
                newShape = document.createElementNS(svgNS, 'ellipse')
                const xRadio = (drawingCoordinates.x2 - drawingCoordinates.x1) / 2
                const yRadio = (drawingCoordinates.y2 - drawingCoordinates.y1) / 2
                const xCenter = drawingCoordinates.x2 - xRadio
                const yCenter = drawingCoordinates.y2 - yRadio
                newShape.setAttribute('rx', xRadio)
                newShape.setAttribute('ry', yRadio)
                newShape.setAttribute('cx', xCenter)
                newShape.setAttribute('cy', yCenter)
                break;
            case 'triangle-shape':
                newShape = document.createElementNS(svgNS, 'polygon')
                const triangleTopPoint = `${drawingCoordinates.x2 - (drawingCoordinates.x2 - drawingCoordinates.x1) / 2},${drawingCoordinates.y1}`
                const triangleRightPoint = `${drawingCoordinates.x2},${drawingCoordinates.y2}`
                const triangleLeftPoint = `${drawingCoordinates.x1},${drawingCoordinates.y2}`
                newShape.setAttribute('points', `${triangleTopPoint} ${triangleRightPoint} ${triangleLeftPoint}`)
                break;
            case 'square-shape':
                newShape = document.createElementNS(svgNS, 'rect')
                newShape.setAttribute('x', drawingCoordinates.x1)
                newShape.setAttribute('y', drawingCoordinates.y1)
                newShape.setAttribute('width', drawingCoordinates.x2 - drawingCoordinates.x1)
                newShape.setAttribute('height', drawingCoordinates.y2 - drawingCoordinates.y1)
                break;
            case 'rhomb-shape':
                newShape = document.createElementNS(svgNS, 'polygon')
                const rhombXRadio = (drawingCoordinates.x2 - drawingCoordinates.x1) / 2
                const rhombYRadio = (drawingCoordinates.y2 - drawingCoordinates.y1) / 2
                const rhombTopPoint = `${drawingCoordinates.x1 + rhombXRadio},${drawingCoordinates.y1}`
                const rhombRightPoint = `${drawingCoordinates.x2},${drawingCoordinates.y1 + rhombYRadio}`
                const rhombBottomPoint = `${drawingCoordinates.x1 + rhombXRadio},${drawingCoordinates.y2}`
                const rhombLeftPoint = `${drawingCoordinates.x1},${drawingCoordinates.y1 + rhombYRadio}`
                newShape.setAttribute('points', `${rhombTopPoint} ${rhombRightPoint} ${rhombBottomPoint} ${rhombLeftPoint}`)
                break;
            default:
                break;
        }

        const lineColor = document.getElementById('line-color-picker').value
        const backgroundColor = document.getElementById('background-color-picker').value
        newShape.setAttribute('style', `stroke:${lineColor};stroke-width:3;fill:${backgroundColor}`)
        svgCanvas.appendChild(newShape)
    } else if (isMoving) {
        const xAxisDiference = movingCoordinates.lastClickX - movingCoordinates.initialClickX
        const yAxisDiference = movingCoordinates.lastClickY - movingCoordinates.initialClickY
        movingCoordinates.drawing.setAttribute('transform', `translate(${xAxisDiference} ${yAxisDiference})`)
    }
}