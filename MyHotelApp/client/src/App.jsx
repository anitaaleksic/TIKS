//import { useState } from 'react'
import './App.css'
import { Routes, Route } from 'react-router-dom';
import Navbar from './Components/Navbar'
import Guest from './Components/Guest'
import Room from './Components/Room';
import RoomService from './Components/RoomService';
import ExtraService from './Components/ExtraService';
import Reservation from './Components/Reservation';
function App() {

  return (
    <>
      <Navbar />

      <Routes>
          <Route path="/guest" element={<Guest />} />
          <Route path="/room" element= {<Room />} />
          <Route path="/roomservice" element= {<RoomService />} />
          <Route path="/extraservice" element= {<ExtraService />} />
          <Route path="/reservation" element= {<Reservation />} />
        </Routes>
    </>
  )
}

export default App
