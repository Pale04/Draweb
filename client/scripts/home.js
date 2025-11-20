const drawingsListElement = document.getElementById('draws-list')

document.addEventListener('DOMContentLoaded', async () => {
    const token = sessionStorage.getItem('jwt_token')
    let id

    if (token) {
        const payload = token.split('.')[1]
        id = JSON.parse(atob(payload)).sid
    } else {
        window.location.replace('index.html')
        return
    }

    try {
        const response = await fetch(`http://localhost:5043/draweb-api/users/${id}/drawings?size=20`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        })

        const responseCode = response.status
        if (responseCode == 200) {
            const responseBody = await response.json()

            for(let i = 0; i < responseBody.length; i++) {
                const svgDrawing = await getSvgDrawing(responseBody[i].drawingId)
                if (!svgDrawing) {
                    return
                }
                svgDrawing.childNodes.forEach((value) => value.setAttribute('transform', 'scale(0.2)'))

                const titleElement = document.createElement('p')
                titleElement.setAttribute('class', 'title')
                titleElement.innerHTML = responseBody[i].title

                const descriptionElement = document.createElement('p')
                descriptionElement.setAttribute('class', 'description')
                descriptionElement.innerHTML = `Last modification: ${new Date(Date.parse(responseBody[i].lastUpdate)).toLocaleDateString('es-MX')}`

                const deleteIcon = document.createElement('img')
                deleteIcon.setAttribute('src', 'icons/trash.png')
                deleteIcon.setAttribute('alt', `Delete ${responseBody[i].title}`)
                const deleteDrawingButton = document.createElement('button')
                deleteDrawingButton.setAttribute('class', 'error-button')
                deleteDrawingButton.addEventListener('click', () => deleteDrawing(responseBody[i].drawingId))
                deleteDrawingButton.appendChild(deleteIcon)

                const optionsPane = document.createElement('span')
                optionsPane.setAttribute('class', 'drawing-item-options-pane')
                optionsPane.appendChild(deleteDrawingButton)
                
                const newDrawing = document.createElement('li')
                newDrawing.appendChild(svgDrawing)
                newDrawing.appendChild(titleElement)
                newDrawing.appendChild(descriptionElement)
                newDrawing.appendChild(optionsPane)
                newDrawing.setAttribute('id', responseBody[i].drawingId)
                newDrawing.setAttribute('class', 'draw-item')
                drawingsListElement.appendChild(newDrawing)
            }
        } else if (responseCode == 401) {
            window.location.replace('index.html')
        } else {
            alert('Something went wrong')
        }
    } catch (e) {
        console.log(e);
        alert('Something went wrong')
    }
});


async function getSvgDrawing(drawingId) {
    const jwt = sessionStorage.getItem('jwt_token')
    if (!jwt) {
        return null
    }

    try {
        const response = await fetch(`http://localhost:5043/draweb-api/users/drawings/${drawingId}`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${jwt}`
            }
        })

        const status = response.status
        if(status == 200) {
            const body = await response.json()
            const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg')
            svg.innerHTML = body.svg
            return svg
        } else if (status == 401) {
            window.location.replace('index.html')
        } else if (status == 404) {
            alert('The drawing was not found')
        } else if (status == 409) {
            alert('The drawing cannot be retrieved because is corrupted')
        } else {
            alert('Something went wrong')
        }
    } catch (e) {
        console.log(e)
        alert('Something went wrong')
    }
}

async function deleteDrawing(drawingId) {
    const jwt = sessionStorage.getItem('jwt_token')
    if (!jwt) {
        return
    }

    try {
        const response = await fetch(`http://localhost:5043/draweb-api/users/drawings/${drawingId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${jwt}`
            }
        })

        const status = response.status
        if(status == 200) {
            drawingsListElement.removeChild(document.getElementById(drawingId))
        } else if (status == 401) {
            window.location.replace('index.html')
        } else if (status == 404) {
            alert('The drawing was not found')
        } else {
            alert('Something went wrong. Try again later')
        }
    } catch (e) {
        console.log(e)
        alert('Something went wrong. Try again later')
    }
}

const newDrawingButton = document.getElementById('new-drawing-button');
newDrawingButton.addEventListener('click', () => {
    window.location.assign('canvas.html')
})

