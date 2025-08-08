import './css/App.css';
import { Routes, Route } from 'react-router-dom';
import Navbar from './Components/Navbar';
import Guest from './Components/GuestComponents/Guest';       
import Room from './Components/RoomComponents/Room';  
import RoomService from './Components/RoomServiceComponents/RoomService';
import AddExtraService from './Components/ExtraServiceComponents/AddExtraService';
import AddGuest from './Components/GuestComponents/AddGuest';   
import AddRoom from './Components/RoomComponents/AddRoom';
import AddRoomService from './Components/RoomServiceComponents/AddRoomService';
import AddReservation from './Components/ReservationComponents/AddReservation';
import ExtraService from './Components/ExtraServiceComponents/ExtraService';
import Reservation from './Components/ReservationComponents/Reservation';
import EditGuest from './Components/GuestComponents/EditGuest';
import InfoGuest from './Components/GuestComponents/InfoGuest';
import InfoRoom from './Components/RoomComponents/InfoRoom';
import EditRoom from './Components/RoomComponents/EditRoom';
import ExtraServiceInfo from './Components/ExtraServiceComponents/InfoExtraService';
import EditExtraService from './Components/ExtraServiceComponents/EditExtraService';
import InfoRoomService from './Components/RoomServiceComponents/InfoRoomService';
import EditRoomService from './Components/RoomServiceComponents/EditRoomService';


function App() {
  return (
    <>
      <Navbar />
      <Routes>
        <Route path="/" element={<Reservation />} />
        <Route path="/guest" element={<Guest />} />        
        <Route path="/addguest" element={<AddGuest />} />  
        <Route path="/room" element={<Room />} />
        <Route path="/addroom" element={<AddRoom />} />
        <Route path="/roomservice" element={<RoomService />} />
        <Route path="/addroomservice" element={<AddRoomService />} />
        <Route path="/extraservice" element={<ExtraService />} />
        <Route path="/addextraservice" element={<AddExtraService />} />
        <Route path="/reservation" element={<Reservation />} />
        <Route path="/addreservation" element={<AddReservation />} />
        <Route path="/editguest/:jmbg" element={<EditGuest />} />
        <Route path="/guestinfo/:jmbg" element={<InfoGuest />} />
        <Route path="/room/info/:roomNumber" element={<InfoRoom />} />
        <Route path="/room/edit/:roomNumber" element={<EditRoom />} />
        <Route path="/extraservice/info/:serviceName" element={<ExtraServiceInfo />} />
        <Route path="/extraservice/edit/:serviceName" element={<EditExtraService />} />
        <Route path="/roomservice/info/:itemName" element={<InfoRoomService />} />
        <Route path="/roomservice/edit/:itemName" element={<EditRoomService />} />
      </Routes>
    </>
  );
}

export default App;
